using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;
using CarMS_API.Models.Dto.UpdateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<Reservation> _reservationRepo;
        private readonly ISearchableRepository<Payment, PaymentSearchParams> _searchRepo;
        private readonly IMapper _mapper;

        public PaymentsController(
            IRepository<Payment> paymentRepo,
            IRepository<Reservation> reservationRepo,
            ISearchableRepository<Payment, 
            PaymentSearchParams> searchRepo,
            IMapper mapper)
        {
            _reservationRepo = reservationRepo;
            _paymentRepo = paymentRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] PaymentSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (payments, totalCount) = await _paymentRepo.GetAllAsync(
                filter,
                orderBy,
                _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<PaymentDto>>.Success(result, "โหลดรายการชำระเงินสำเร็จ", pagination));
        }

        [HttpGet("getbyid/{paymentId}")]
        public async Task<IActionResult> GetById(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q=> q
            .Include(p => p.Reservation)
            .ThenInclude(r => r.User));

            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบชำระเงินที่คุณค้นหา"));
            var result = _mapper.Map<PaymentDto>(payment);

            return Ok(ApiResponse<PaymentDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto paymentDto)
        {
            // ถ้ามี Payment pending อยู่แล้ว ไม่ต้องให้สร้างซ้ำ
            var pendingPayment = await _paymentRepo.FirstOrDefaultAsync(p =>
                p.ReservationId == paymentDto.ReservationId &&
                p.Status == PaymentStatus.Pending);

            if (pendingPayment != null)
                return BadRequest(ApiResponse<string>.Fail("มีรายการชำระเงินที่รอดำเนินการอยู่แล้ว"));

            var reservation = await _reservationRepo.GetByIdAsync(paymentDto.ReservationId, 
                r => r.Include(r => r.Car));

            if (reservation == null || reservation.Status != ReservationStatus.Pending)
                return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะไม่สามารถชำระเงินได้"));

            if (reservation.ExpiryAt < DateTime.UtcNow)
                return BadRequest(ApiResponse<string>.Fail("การจองหมดอายุแล้ว ไม่สามารถชำระเงินได้"));

            var existingPayment = await _paymentRepo.FirstOrDefaultAsync(p =>
                p.ReservationId == paymentDto.ReservationId &&
                p.Status == PaymentStatus.Paid);

            if (existingPayment != null)
                return BadRequest(ApiResponse<string>.Fail("มีการชำระเงินสำหรับการจองนี้แล้ว"));

            var expectedPrice = reservation.Car.Price;
            if (paymentDto.TotalPrice != expectedPrice)
                return BadRequest(ApiResponse<string>.Fail($"ยอดเงินที่ต้องชำระคือ {expectedPrice} บาท"));

            var payment = _mapper.Map<Payment>(paymentDto);
            payment.CreatedAt = DateTime.UtcNow;
            payment.Status = PaymentStatus.Pending;

            reservation.Status = ReservationStatus.PendingPayment;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _reservationRepo.UpdateAsync(reservation);

            var created = await _paymentRepo.AddAsync(payment);
            var result = _mapper.Map<PaymentDto>(payment);

            return Ok(ApiResponse<PaymentDto>.Success(result, "สร้างรายการชำระเงินสำเร็จ"));
        }

        [HttpPost("confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q => q.Include(p => p.Reservation));
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.Status == PaymentStatus.Paid)
                return BadRequest(ApiResponse<string>.Fail("รายการนี้ถูกชำระเงินแล้ว"));

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Reservation.Status = ReservationStatus.Confirmed;
            payment.Reservation.UpdatedAt = DateTime.UtcNow;
            await _reservationRepo.UpdateAsync(payment.Reservation);

            await _paymentRepo.UpdateAsync(payment);
            return Ok(ApiResponse<string>.Success("ยืนยันการชำระเงินสำเร็จ"));
        }

        [HttpPut("update/{paymentId}")]
        public async Task<IActionResult> Update([FromBody] PaymentUpdateDto paymentDto, int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.Status == PaymentStatus.Paid)
                return BadRequest(ApiResponse<string>.Fail("ไม่สามารถแก้ไขรายการที่ชำระเงินแล้วได้"));

            payment.TotalPrice = paymentDto.TotalPrice;
            payment.Method = paymentDto.Method;
            payment.TransactionRef = paymentDto.TransactionRef;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepo.UpdateAsync(payment);
            var result = _mapper.Map<PaymentDto>(payment);
            return Ok(ApiResponse<PaymentDto>.Success(result, "อัปเดตรายการชำระเงินเรียบร้อย"));
        }
    }
}