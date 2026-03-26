using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
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
    public class StripePaymentsController : ControllerBase
    {
        private readonly IRepository<Booking> _BookingRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly string _webhookSecret;
        private readonly IHubContext<NotificationHub> _hubContext;

        public StripePaymentsController(
            IRepository<Booking> BookingRepo,
             IRepository<Payment> paymentRepo,
            IConfiguration config,
            IHubContext<NotificationHub> hubContext)

        {
            _BookingRepo = BookingRepo;
            _paymentRepo = paymentRepo;
            _webhookSecret = config["StripeSettings:WebhookSecret"];
            _hubContext = hubContext;
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreateIntent([FromBody] PaymentIntentCreateDto dto)
        {
            var Booking = await _BookingRepo.GetByIdAsync(dto.BookingId, r => r.Include(r => r.Car));

            if (Booking == null || Booking.BookingStatus != SD.Booking_Pending)
                return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะการจองไม่ถูกต้อง (อาจหมดอายุหรือจ่ายแล้ว)"));

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

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;

            try { stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookSecret); }
            catch (Exception) { return BadRequest("Invalid webhook signature."); }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var intent = stripeEvent.Data.Object as PaymentIntent;

                if (await _paymentRepo.FirstOrDefaultAsync(p => p.TransactionRef == intent.Id) != null)
                    return Ok();

                if (!intent.Metadata.TryGetValue("BookingId", out var BookingIdStr) || !int.TryParse(BookingIdStr, out var BookingId))
                    return BadRequest("Invalid or missing BookingId in metadata");

                // 🌟 ดึงข้อมูล User และ Seller มาด้วยเพื่อส่งแจ้งเตือน
                var Booking = await _BookingRepo.GetByIdAsync(BookingId, r => 
                    r.Include(b => b.User)
                     .Include(b => b.Car).ThenInclude(c => c.Seller));
                     
                if (Booking == null) return BadRequest("Booking not found");

                var expectedAmount = (long)(Booking.Car.BookingPrice * 100);
                if (intent.AmountReceived != expectedAmount) return BadRequest("Amount mismatch");

                Booking.BookingStatus = SD.Booking_Confirmed;
                Booking.UpdatedAt = DateTime.UtcNow;
                await _BookingRepo.UpdateAsync(Booking);

                var payment = new Payment
                {
                    BookingId = Booking.Id,
                    PaymentMethod = SD.PaymentMethod_CreditCard,
                    TotalPrice = Booking.Car.BookingPrice, 
                    PaymentStatus = SD.Payment_Paid, 
                    PaidAt = DateTime.UtcNow,
                    TransactionRef = intent.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _paymentRepo.AddAsync(payment);

                // ⚡ สั่ง SignalR: แจ้งเตือน "ลูกค้า" (ว่าจ่ายผ่านแล้ว)
                if (!string.IsNullOrEmpty(Booking.UserId))
                {
                    await _hubContext.Clients.Group(Booking.UserId).SendAsync("ReceiveNotification", new 
                    {
                        Title = "ชำระเงินสำเร็จ!",
                        Message = $"ระบบได้รับยอดชำระค่าจองรถ {Booking.Car?.Model} ของคุณเรียบร้อยแล้ว",
                        BookingId = Booking.Id
                    });
                }

                // ⚡ สั่ง SignalR: แจ้งเตือน "ผู้ขาย" (ว่ามีคนจ่ายเงินแล้ว)
                if (Booking.Car?.Seller != null && !string.IsNullOrEmpty(Booking.Car.Seller.UserId))
                {
                    await _hubContext.Clients.Group(Booking.Car.Seller.UserId).SendAsync("ReceiveNotification", new 
                    {
                        Title = "ได้รับยอดชำระเงินค่าจอง!",
                        Message = $"ลูกค้าชำระค่าจองรถ {Booking.Car.Model} ผ่านบัตรเครดิตเรียบร้อยแล้ว",
                        BookingId = Booking.Id
                    });
                }
            }

            return Ok();
        }

        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] string transactionRef)
        {
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

                // ⚡ สั่ง SignalR: แจ้งเตือนลูกค้าว่าคืนเงินแล้ว
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
    }
}