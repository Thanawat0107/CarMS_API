using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;
using CarMS_API.Models.Dto.UpdateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Services.IServices;
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
        private readonly IMapper _mapper;
        private readonly IFileUpload _fileUpload; // 🌟 เพิ่ม FileUpload

        public PaymentsController(
            IRepository<Payment> paymentRepo,
            IRepository<Booking> BookingRepo,
            IMapper mapper,
            IFileUpload fileUpload) // 🌟 Inject เข้ามา
        {
            _BookingRepo = BookingRepo;
            _paymentRepo = paymentRepo;
            _mapper = mapper;
            _fileUpload = fileUpload;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var (payments, totalCount) = await _paymentRepo.GetAllAsync(
                include: query => query.Include(q => q.Booking),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<PaymentDto>>.Success(result, "โหลดรายการชำระเงินสำเร็จ", meta));
        }

        [HttpGet("getbyid/{paymentId}")]
        public async Task<IActionResult> GetById(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q => q
            .Include(p => p.Booking)
            .ThenInclude(r => r.User));

            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบชำระเงินที่คุณค้นหา"));
            var result = _mapper.Map<PaymentDto>(payment);

            return Ok(ApiResponse<PaymentDto>.Success(result, "สำเร็จ"));
        }

        // การจ่ายแบบ Manual (อัปโหลดสลิป)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] PaymentCreateDto paymentDto) // 🌟 เปลี่ยนเป็น FromForm เพื่อรับไฟล์สลิป
        {
            var allowedMethods = new[] { SD.PaymentMethod_QR, SD.PaymentMethod_PromptPay, SD.PaymentMethod_BankTransfer };
            if (!allowedMethods.Contains(paymentDto.PaymentMethod))
                return BadRequest(ApiResponse<string>.Fail("ช่องทางการชำระเงินไม่ถูกต้อง"));

            var Booking = await _BookingRepo.GetByIdAsync(paymentDto.BookingId, r => r.Include(r => r.Car));
            if (Booking == null || Booking.BookingStatus != SD.Booking_Pending)
                return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะไม่สามารถชำระเงินได้"));

            if (Booking.ExpiryAt < DateTime.UtcNow)
                return BadRequest(ApiResponse<string>.Fail("การจองหมดอายุแล้ว ไม่สามารถชำระเงินได้"));

            var existingPayment = await _paymentRepo.FirstOrDefaultAsync(p =>
                p.BookingId == paymentDto.BookingId && 
                (p.PaymentStatus == SD.Payment_Paid || p.PaymentStatus == SD.Payment_Verifying));

            if (existingPayment != null)
                return BadRequest(ApiResponse<string>.Fail("มีการชำระเงิน หรือมีสลิปที่รอตรวจสอบสำหรับการจองนี้แล้ว"));

            // 🌟 แก้ไข: เทียบราคากับ BookingPrice (ค่าจอง) ไม่ใช่ราคาเต็มรถ
            var expectedPrice = Booking.Car.BookingPrice;
            if (paymentDto.TotalPrice != expectedPrice)
                return BadRequest(ApiResponse<string>.Fail($"ยอดเงินที่ต้องชำระ (ค่าจอง) คือ {expectedPrice} บาท"));

            var payment = _mapper.Map<Payment>(paymentDto);
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            // 🌟 จัดการอัปโหลดไฟล์สลิป
            if (paymentDto.SlipImage != null)
            {
                payment.SlipImageUrl = await _fileUpload.UploadFile(paymentDto.SlipImage, SD.ImgPaymentPath);
                payment.PaymentStatus = SD.Payment_Verifying; // มีสลิปแล้ว ให้สถานะเป็นรอตรวจสอบ
                Booking.BookingStatus = SD.Booking_PendingPayment; // อัปเดตสถานะการจอง
            }
            else
            {
                payment.PaymentStatus = SD.Payment_Pending; // ถ้ายังไม่แนบสลิป ก็ให้สถานะรอจ่ายไปก่อน
            }

            Booking.UpdatedAt = DateTime.UtcNow;
            await _BookingRepo.UpdateAsync(Booking);

            var created = await _paymentRepo.AddAsync(payment);
            var result = _mapper.Map<PaymentDto>(payment);

            return Ok(ApiResponse<PaymentDto>.Success(result, "สร้างรายการชำระเงินสำเร็จ"));
        }

        // สำหรับ Admin หรือ Seller กด "ยืนยันว่าสลิปถูกต้อง"
        [HttpPost("confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q => q.Include(p => p.Booking));
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.PaymentStatus == SD.Payment_Paid)
                return BadRequest(ApiResponse<string>.Fail("รายการนี้ถูกชำระเงินแล้ว"));

            payment.PaymentStatus = SD.Payment_Paid; // เปลี่ยนเป็น Paid
            payment.PaidAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Booking.BookingStatus = SD.Booking_Confirmed; // เปลี่ยนสถานะจองเป็น Confirmed
            payment.Booking.UpdatedAt = DateTime.UtcNow;
            await _BookingRepo.UpdateAsync(payment.Booking);

            await _paymentRepo.UpdateAsync(payment);
            return Ok(ApiResponse<string>.Success("ยืนยันการชำระเงินสำเร็จ"));
        }

        // กรณีลูกค้าอัปโหลดสลิปผิด แล้วอยากอัปเดตใหม่
        [HttpPut("update/{paymentId}")]
        public async Task<IActionResult> Update([FromForm] PaymentUpdateDto paymentDto, int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.PaymentStatus == SD.Payment_Paid)
                return BadRequest(ApiResponse<string>.Fail("ไม่สามารถแก้ไขรายการที่ชำระเงินแล้วได้"));

            // 🌟 ถ้ามีการส่งสลิปมาใหม่
            if (paymentDto.SlipImage != null)
            {
                if (!string.IsNullOrEmpty(payment.SlipImageUrl))
                {
                    _fileUpload.DeleteFile(payment.SlipImageUrl); // ลบสลิปเก่าทิ้ง
                }
                payment.SlipImageUrl = await _fileUpload.UploadFile(paymentDto.SlipImage, SD.ImgPaymentPath);
                payment.PaymentStatus = SD.Payment_Verifying; // กลับไปสถานะรอตรวจสอบใหม่
            }

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