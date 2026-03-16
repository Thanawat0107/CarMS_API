using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<Booking> _BookingRepo;
        //private readonly ISearchableRepository<Payment, PaymentSearchParams> _searchRepo;
        private readonly IMapper _mapper;

        public PaymentsController(
            IRepository<Payment> paymentRepo,
            IRepository<Booking> BookingRepo,
            //ISearchableRepository<Payment, 
            //PaymentSearchParams> searchRepo,
            IMapper mapper)
        {
            _BookingRepo = BookingRepo;
            _paymentRepo = paymentRepo;
            //_searchRepo = searchRepo;
            _mapper = mapper;
        }

        //[HttpGet("getall")]
        //public async Task<IActionResult> GetAll([FromQuery] PaymentSearchParams searchParams)
        //{
        //    var filter = _searchRepo.BuildFilter(searchParams);
        //    var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

        //    var (payments, totalCount) = await _paymentRepo.GetAllAsync(
        //        filter,
        //        orderBy,
        //        _searchRepo.Include(),
        //        searchParams.PageNumber,
        //        searchParams.PageSize
        //    );

        //    var result = _mapper.Map<IEnumerable<PaymentDto>>(payments);

        //    var pagination = new PaginationMeta
        //    {
        //        TotalCount = totalCount,
        //        PageNumber = searchParams.PageNumber,
        //        PageSize = searchParams.PageSize
        //    };

        //    return Ok(ApiResponse<IEnumerable<PaymentDto>>.Success(result, "โหลดรายการชำระเงินสำเร็จ", pagination));
        //}

        [HttpGet("getbyid/{paymentId}")]
        public async Task<IActionResult> GetById(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q=> q
            .Include(p => p.Booking)
            .ThenInclude(r => r.User));

            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบชำระเงินที่คุณค้นหา"));
            var result = _mapper.Map<PaymentDto>(payment);

            return Ok(ApiResponse<PaymentDto>.Success(result, "สำเร็จ"));
        }

        // การจ่ายแบบ Manual (โอนเงิน, QR code, อื่นๆ)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto paymentDto)
        {
            var allowedMethods = new[] { SD.PaymentMethod_QR, SD.PaymentMethod_PromptPay };
            if (!allowedMethods.Contains(paymentDto.PaymentMethod))
                return BadRequest(ApiResponse<string>.Fail("ช่องทางการชำระเงินไม่ถูกต้อง"));

            // ถ้ามี Payment pending อยู่แล้ว ไม่ต้องให้สร้างซ้ำ
            var pendingPayment = await _paymentRepo.FirstOrDefaultAsync(p =>
                p.BookingId == paymentDto.BookingId &&
                p.PaymentStatus == SD.Payment_Pending);

            if (pendingPayment != null)
                return BadRequest(ApiResponse<string>.Fail("มีรายการชำระเงินที่รอดำเนินการอยู่แล้ว"));

            var Booking = await _BookingRepo.GetByIdAsync(paymentDto.BookingId, 
                r => r.Include(r => r.Car));

            if (Booking == null || Booking.BookingStatus != SD.Reserve_Pending)
                return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะไม่สามารถชำระเงินได้"));

            if (Booking.ExpiryAt < DateTime.UtcNow)
                return BadRequest(ApiResponse<string>.Fail("การจองหมดอายุแล้ว ไม่สามารถชำระเงินได้"));

            var existingPayment = await _paymentRepo.FirstOrDefaultAsync(p =>
                p.BookingId == paymentDto.BookingId &&
                p.PaymentStatus == SD.Payment_Paid);

            if (existingPayment != null)
                return BadRequest(ApiResponse<string>.Fail("มีการชำระเงินสำหรับการจองนี้แล้ว"));

            var expectedPrice = Booking.Car.Price;
            if (paymentDto.TotalPrice != (double)expectedPrice)
                return BadRequest(ApiResponse<string>.Fail($"ยอดเงินที่ต้องชำระคือ {expectedPrice} บาท"));

            var payment = _mapper.Map<Payment>(paymentDto);
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.PaymentStatus = SD.Payment_Pending;

            Booking.BookingStatus = SD.Reserve_PendingPayment;
            Booking.UpdatedAt = DateTime.UtcNow;
            await _BookingRepo.UpdateAsync(Booking);

            var created = await _paymentRepo.AddAsync(payment);
            var result = _mapper.Map<PaymentDto>(payment);

            return Ok(ApiResponse<PaymentDto>.Success(result, "สร้างรายการชำระเงินสำเร็จ"));
        }

        [HttpPost("confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q => q.Include(p => p.Booking));
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.PaymentStatus == SD.Payment_Paid)
                return BadRequest(ApiResponse<string>.Fail("รายการนี้ถูกชำระเงินแล้ว"));

            payment.PaymentStatus = SD.Payment_Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Booking.BookingStatus = SD.Reserve_Confirmed;
            payment.Booking.UpdatedAt = DateTime.UtcNow;
            await _BookingRepo.UpdateAsync(payment.Booking);

            await _paymentRepo.UpdateAsync(payment);
            return Ok(ApiResponse<string>.Success("ยืนยันการชำระเงินสำเร็จ"));
        }

        [HttpPut("update/{paymentId}")]
        public async Task<IActionResult> Update([FromBody] PaymentUpdateDto paymentDto, int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.PaymentStatus == SD.Payment_Paid)
                return BadRequest(ApiResponse<string>.Fail("ไม่สามารถแก้ไขรายการที่ชำระเงินแล้วได้"));

            payment.TotalPrice = (decimal)paymentDto.TotalPrice;
            payment.PaymentMethod = paymentDto.PaymentMethod;
            payment.TransactionRef = paymentDto.TransactionRef;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepo.UpdateAsync(payment);
            var result = _mapper.Map<PaymentDto>(payment);
            return Ok(ApiResponse<PaymentDto>.Success(result, "อัปเดตรายการชำระเงินเรียบร้อย"));
        }
    }
}