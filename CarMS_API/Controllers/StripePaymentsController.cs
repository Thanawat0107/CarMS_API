using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Microsoft.EntityFrameworkCore;
using CarMS_API.Utility;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Responsts;
using Microsoft.AspNetCore.SignalR;
using CarMS_API.Hubs;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StripePaymentsController : ControllerBase
    {
        private readonly IRepository<Booking> _BookingRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IHubContext<NotificationHub> _hubContext;

        public StripePaymentsController(
            IRepository<Booking> BookingRepo,
            IRepository<Payment> paymentRepo,
            IHubContext<NotificationHub> hubContext)
        {
            _BookingRepo = BookingRepo;
            _paymentRepo = paymentRepo;
            _hubContext = hubContext;
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreateIntent([FromBody] PaymentIntentCreateDto dto)
        {
            try
            {
                var Booking = await _BookingRepo.GetByIdAsync(dto.BookingId, r => r.Include(r => r.Car));

                if (Booking == null ||
                    (Booking.BookingStatus != SD.Booking_Pending && Booking.BookingStatus != SD.Booking_PendingPayment))
                    return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะการจองไม่ถูกต้อง (อาจหมดอายุหรือจ่ายแล้ว)"));

                if (Booking.Car == null)
                    return BadRequest(ApiResponse<string>.Fail("ไม่พบข้อมูลรถยนต์ที่เกี่ยวข้อง"));

                // ล็อกสถานะเฉพาะครั้งแรก (ป้องกัน double-call จาก React StrictMode)
                if (Booking.BookingStatus == SD.Booking_Pending)
                {
                    Booking.BookingStatus = SD.Booking_PendingPayment;
                    Booking.UpdatedAt = DateTime.UtcNow;
                    await _BookingRepo.UpdateAsync(Booking);
                }

                var amountInCents = (long)(Booking.Car.BookingPrice * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = "thb", 
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", Booking.Id.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                return Ok(ApiResponse<object>.Success(new
                {
                    clientSecret = intent.ClientSecret,
                }, "สร้าง Payment Intent สำเร็จ"));
            }
            catch (StripeException stripeEx)
            {
                // 🟢 ดัก Error จาก Stripe
                return BadRequest(ApiResponse<string>.Fail($"ระบบชำระเงินมีปัญหา: {stripeEx.Message}"));
            }
            catch (Exception ex)
            {
                // 🟢 ดัก Error จากระบบ (เช่น DB พัง)
                return StatusCode(500, ApiResponse<string>.Fail($"เกิดข้อผิดพลาดภายในระบบ: {ex.Message}"));
            }
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmDto dto)
        {
            try
            {
                var service = new PaymentIntentService();
                var intent = await service.GetAsync(dto.PaymentIntentId);

                if (intent == null || intent.Status != "succeeded")
                    return BadRequest(ApiResponse<string>.Fail("การชำระเงินยังไม่สำเร็จ"));

                // idempotency — กันสร้าง Payment ซ้ำ
                if (await _paymentRepo.FirstOrDefaultAsync(p => p.TransactionRef == intent.Id) != null)
                    return Ok(ApiResponse<string>.Success("ยืนยันแล้ว", "ชำระเงินและยืนยันการจองสำเร็จ"));

                if (intent.Metadata == null || !intent.Metadata.TryGetValue("BookingId", out var bookingIdStr) || !int.TryParse(bookingIdStr, out var bookingId))
                    return BadRequest(ApiResponse<string>.Fail("ไม่พบข้อมูลการจองใน PaymentIntent"));

                var booking = await _BookingRepo.GetByIdAsync(bookingId, r =>
                    r.Include(b => b.User)
                     .Include(b => b.Car).ThenInclude(c => c.Seller));

                if (booking == null) return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง"));

                var expectedAmount = (long)(booking.Car.BookingPrice * 100);
                if (intent.AmountReceived != expectedAmount)
                    return BadRequest(ApiResponse<string>.Fail("ยอดเงินไม่ตรงกัน"));

                booking.BookingStatus = SD.Booking_Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                // 🟢 เพิ่ม: เปลี่ยนสถานะรถไม่ให้คนอื่นจองซ้ำได้
                if (booking.Car != null)
                {
                    booking.Car.CarStatus = SD.Status_Booked; // เช็คในไฟล์ SD.cs ด้วยว่ามี Status_Booked หรือใช้ชื่อตัวแปรอื่นครับ
                }

                await _BookingRepo.UpdateAsync(booking);

                var payment = new Payment
                {
                    BookingId = booking.Id,
                    PaymentMethod = SD.PaymentMethod_CreditCard,
                    TotalPrice = booking.Car.BookingPrice,
                    PaymentStatus = SD.Payment_Paid,
                    PaidAt = DateTime.UtcNow,
                    TransactionRef = intent.Id,
                    SlipImageUrl = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _paymentRepo.AddAsync(payment);

                if (!string.IsNullOrEmpty(booking.UserId))
                    await _hubContext.Clients.Group(booking.UserId).SendAsync("ReceiveNotification", new
                    {
                        Title = "ชำระเงินสำเร็จ!",
                        Message = $"ระบบได้รับยอดชำระค่าจองรถ {booking.Car?.Model} ของคุณเรียบร้อยแล้ว",
                        BookingId = booking.Id
                    });

                if (booking.Car?.Seller != null && !string.IsNullOrEmpty(booking.Car.Seller.UserId))
                    await _hubContext.Clients.Group(booking.Car.Seller.UserId).SendAsync("ReceiveNotification", new
                    {
                        Title = "ได้รับยอดชำระเงินค่าจอง!",
                        Message = $"ลูกค้าชำระค่าจองรถ {booking.Car.Model} ผ่านบัตรเครดิตเรียบร้อยแล้ว",
                        BookingId = booking.Id
                    });

                return Ok(ApiResponse<string>.Success("ยืนยันสำเร็จ", "ชำระเงินและยืนยันการจองสำเร็จ"));
            }
            catch (StripeException stripeEx)
            {
                return BadRequest(ApiResponse<string>.Fail($"ระบบชำระเงินมีปัญหา: {stripeEx.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"เกิดข้อผิดพลาดภายในระบบ: {ex.Message}"));
            }
        }


        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundRequestDto dto)
        {
            try
            {
                var transactionRef = dto.TransactionRef;
                
                var payment = await _paymentRepo.FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);
                if (payment == null || payment.PaymentStatus != SD.Payment_Paid)
                    return BadRequest("ไม่พบรายการชำระเงิน หรือชำระเงินยังไม่สำเร็จ");

                var refundOptions = new RefundCreateOptions { PaymentIntent = transactionRef };
                var refundService = new RefundService();
                await refundService.CreateAsync(refundOptions);

                payment.PaymentStatus = SD.Payment_Refunded;
                payment.UpdatedAt = DateTime.UtcNow;
                
                var booking = await _BookingRepo.GetByIdAsync(payment.BookingId, r => r.Include(b => b.User).Include(b => b.Car));
                if (booking != null)
                {
                    booking.BookingStatus = SD.Booking_Canceled;
                    booking.CanceledAt = DateTime.UtcNow;
                    if (booking.Car != null)
                    {
                        booking.Car.CarStatus = SD.Status_Available; 
                    }
                    await _BookingRepo.UpdateAsync(booking);

                    // ⚡ สั่ง SignalR: แจ้งเตือนลูกค้า
                    if (!string.IsNullOrEmpty(booking.UserId))
                    {
                        await _hubContext.Clients.Group(booking.UserId).SendAsync("ReceiveNotification", new 
                        {
                            Title = "คืนเงินสำเร็จ",
                            Message = $"ระบบได้ทำการคืนเงินค่าจองรถ {booking.Car?.Model} และยกเลิกการจองเรียบร้อยแล้ว",
                            BookingId = booking.Id
                        });
                    }
                }

                await _paymentRepo.UpdateAsync(payment);
                return Ok("ทำการคืนเงินและยกเลิกการจองเรียบร้อย");
            }
            catch (StripeException stripeEx)
            {
                return BadRequest($"ระบบชำระเงินมีปัญหา: {stripeEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"เกิดข้อผิดพลาดภายในระบบ: {ex.Message}");
            }
        }

        public class ConfirmDto
        {
            public string PaymentIntentId { get; set; } = string.Empty;
        }

        public class RefundRequestDto
        {
            public string TransactionRef { get; set; } = string.Empty;
        }
    }
}