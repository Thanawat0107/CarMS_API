using AutoMapper;
using CarMS_API.Hubs;
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
using Microsoft.AspNetCore.SignalR;
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
        private readonly IFileUpload _fileUpload;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PaymentsController(
            IRepository<Payment> paymentRepo,
            IRepository<Booking> BookingRepo,
            IMapper mapper,
            IFileUpload fileUpload,
            IHubContext<NotificationHub> hubContext)
        {
            _BookingRepo = BookingRepo;
            _paymentRepo = paymentRepo;
            _mapper = mapper;
            _fileUpload = fileUpload;
            _hubContext = hubContext;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(string? userId, int? sellerId, int pageNumber = 1, int pageSize = 10)
        {
            var (payments, totalCount) = await _paymentRepo.GetAllAsync(
                filter: q => (string.IsNullOrEmpty(userId) || q.Booking.UserId == userId) &&
                             (!sellerId.HasValue || q.Booking.Car.SellerId == sellerId.Value),
                include: query => query.Include(q => q.Booking).ThenInclude(b => b.Car),
                pageNumber: pageNumber,
                pageSize: pageSize
            );
            var result = _mapper.Map<IEnumerable<PaymentDto>>(payments);
            var meta = new PaginationMeta { TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
            return Ok(ApiResponse<IEnumerable<PaymentDto>>.Success(result, "โหลดรายการชำระเงินสำเร็จ", meta));
        }

        [HttpGet("getbyid/{paymentId}")]
        public async Task<IActionResult> GetById(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q => q.Include(p => p.Booking).ThenInclude(r => r.User));
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบชำระเงินที่คุณค้นหา"));
            return Ok(ApiResponse<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment), "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] PaymentCreateDto paymentDto) 
        {
            var allowedMethods = new[] { SD.PaymentMethod_QR, SD.PaymentMethod_PromptPay, SD.PaymentMethod_BankTransfer };
            if (!allowedMethods.Contains(paymentDto.PaymentMethod))
                return BadRequest(ApiResponse<string>.Fail("ช่องทางการชำระเงินไม่ถูกต้อง"));

            // 🌟 Include Seller มาเพื่อแจ้งเตือน
            var Booking = await _BookingRepo.GetByIdAsync(paymentDto.BookingId, r => r.Include(b => b.Car).ThenInclude(c => c.Seller));
            if (Booking == null || (Booking.BookingStatus != SD.Booking_Pending && Booking.BookingStatus != SD.Booking_PendingPayment))
                return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะไม่สามารถชำระเงินได้"));

            if (Booking.ExpiryAt < DateTime.UtcNow)
                return BadRequest(ApiResponse<string>.Fail("การจองหมดอายุแล้ว ไม่สามารถชำระเงินได้"));

            var existingPayment = await _paymentRepo.FirstOrDefaultAsync(p =>
                p.BookingId == paymentDto.BookingId && 
                (p.PaymentStatus == SD.Payment_Paid || p.PaymentStatus == SD.Payment_Verifying));

            if (existingPayment != null)
                return BadRequest(ApiResponse<string>.Fail("มีการชำระเงิน หรือมีสลิปที่รอตรวจสอบสำหรับการจองนี้แล้ว"));

            var expectedPrice = Booking.Car.BookingPrice;
            if (paymentDto.TotalPrice != expectedPrice)
                return BadRequest(ApiResponse<string>.Fail($"ยอดเงินที่ต้องชำระ (ค่าจอง) คือ {expectedPrice} บาท"));

            var payment = _mapper.Map<Payment>(paymentDto);
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            if (paymentDto.SlipImage != null)
            {
                payment.SlipImageUrl = await _fileUpload.UploadFile(paymentDto.SlipImage, SD.ImgPaymentPath);
                payment.PaymentStatus = SD.Payment_Verifying; 
                Booking.BookingStatus = SD.Booking_PendingPayment; 

                // ⚡ สั่ง SignalR: แจ้งเตือน "ผู้ขาย" ว่ามีคนแนบสลิปมาให้ตรวจ
                if (Booking.Car?.Seller != null && !string.IsNullOrEmpty(Booking.Car.Seller.UserId))
                {
                    await _hubContext.Clients.Group(Booking.Car.Seller.UserId).SendAsync("ReceiveNotification", new 
                    {
                        Title = "แนบสลิปชำระเงินใหม่",
                        Message = $"มีลูกค้าแนบสลิปค่าจองรถ {Booking.Car.Model} กรุณาตรวจสอบยอดเงิน",
                        BookingId = Booking.Id
                    });
                }
            }
            else
            {
                payment.PaymentStatus = SD.Payment_Pending; 
            }

            Booking.UpdatedAt = DateTime.UtcNow;
            await _BookingRepo.UpdateAsync(Booking);

            var created = await _paymentRepo.AddAsync(payment);
            var result = _mapper.Map<PaymentDto>(payment);

            return Ok(ApiResponse<PaymentDto>.Success(result, "สร้างรายการชำระเงินสำเร็จ"));
        }

        [HttpPost("confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            // 🌟 Include User มาเพื่อแจ้งเตือน
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q => q.Include(p => p.Booking).ThenInclude(b => b.User).Include(p => p.Booking.Car));
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.PaymentStatus == SD.Payment_Paid)
                return BadRequest(ApiResponse<string>.Fail("รายการนี้ถูกชำระเงินแล้ว"));

            payment.PaymentStatus = SD.Payment_Paid; 
            payment.PaidAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Booking.BookingStatus = SD.Booking_Confirmed; 
            payment.Booking.UpdatedAt = DateTime.UtcNow;
            await _BookingRepo.UpdateAsync(payment.Booking);

            await _paymentRepo.UpdateAsync(payment);

            // ⚡ สั่ง SignalR: แจ้งเตือน "ลูกค้า" ว่าสลิปผ่านแล้ว
            if (!string.IsNullOrEmpty(payment.Booking?.UserId))
            {
                await _hubContext.Clients.Group(payment.Booking.UserId).SendAsync("ReceiveNotification", new 
                {
                    Title = "สลิปได้รับการยืนยัน!",
                    Message = $"สลิปชำระค่าจองรถ {payment.Booking.Car?.Model} ของคุณได้รับการอนุมัติแล้ว",
                    BookingId = payment.BookingId
                });
            }

            return Ok(ApiResponse<string>.Success("ยืนยันการชำระเงินสำเร็จ"));
        }

        [HttpPut("update/{paymentId}")]
        public async Task<IActionResult> Update([FromForm] PaymentUpdateDto paymentDto, int paymentId)
        {
            // 🌟 Include ไปถึง Seller
            var payment = await _paymentRepo.GetByIdAsync(paymentId, q => q.Include(p => p.Booking).ThenInclude(b => b.Car).ThenInclude(c => c.Seller));
            if (payment == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการชำระเงิน"));

            if (payment.PaymentStatus == SD.Payment_Paid)
                return BadRequest(ApiResponse<string>.Fail("ไม่สามารถแก้ไขรายการที่ชำระเงินแล้วได้"));

            if (paymentDto.SlipImage != null)
            {
                if (!string.IsNullOrEmpty(payment.SlipImageUrl))
                {
                    _fileUpload.DeleteFile(payment.SlipImageUrl); 
                }
                payment.SlipImageUrl = await _fileUpload.UploadFile(paymentDto.SlipImage, SD.ImgPaymentPath);
                payment.PaymentStatus = SD.Payment_Verifying; 

                // ⚡ สั่ง SignalR: แจ้งเตือน "ผู้ขาย" ว่าลูกค้าแก้สลิปใหม่
                if (payment.Booking?.Car?.Seller != null && !string.IsNullOrEmpty(payment.Booking.Car.Seller.UserId))
                {
                    await _hubContext.Clients.Group(payment.Booking.Car.Seller.UserId).SendAsync("ReceiveNotification", new 
                    {
                        Title = "อัปเดตสลิปชำระเงิน",
                        Message = $"ลูกค้าทำการอัปเดตสลิปค่าจองรถ {payment.Booking.Car.Model} กรุณาตรวจสอบอีกครั้ง",
                        BookingId = payment.BookingId
                    });
                }
            }

            payment.TotalPrice = paymentDto.TotalPrice;
            payment.PaymentMethod = paymentDto.PaymentMethod;
            payment.TransactionRef = paymentDto.TransactionRef;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepo.UpdateAsync(payment);
            var result = _mapper.Map<PaymentDto>(payment);
            return Ok(ApiResponse<PaymentDto>.Success(result, "อัปเดตรายการชำระเงินเรียบร้อย"));
        }
    }
}