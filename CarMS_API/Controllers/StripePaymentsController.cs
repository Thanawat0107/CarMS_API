using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Microsoft.EntityFrameworkCore;
using CarMS_API.Utility;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Responsts;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripePaymentsController : ControllerBase
    {
        private readonly IRepository<Booking> _BookingRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly string _webhookSecret;

        public StripePaymentsController(
            IRepository<Booking> BookingRepo,
             IRepository<Payment> paymentRepo,
            IConfiguration config)
        {
            _BookingRepo = BookingRepo;
            _paymentRepo = paymentRepo;
            _webhookSecret = config["StripeSettings:WebhookSecret"];
        }

        // 🌟 ปลดคอมเมนต์และปรับแก้: การสร้าง Payment Intent เพื่อส่ง ClientSecret กลับไปให้หน้าเว็บ
        [HttpPost("create-intent")]
        public async Task<IActionResult> CreateIntent([FromBody] PaymentIntentCreateDto dto)
        {
            var Booking = await _BookingRepo.GetByIdAsync(dto.BookingId, r => r.Include(r => r.Car));

            if (Booking == null || Booking.BookingStatus != SD.Booking_Pending)
                return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะการจองไม่ถูกต้อง (อาจหมดอายุหรือจ่ายแล้ว)"));

            // Stripe รับหน่วยเป็นสตางค์ (Cents) ดังนั้นต้องเอา BookingPrice (ค่าจอง) มา * 100
            var amountInCents = (long)(Booking.Car.BookingPrice * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "thb", // หรือสกุลเงินที่ต้องการ
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

        // Webhook Stripe (เมื่อจ่ายเงินสำเร็จ)
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookSecret);
            }
            catch (Exception)
            {
                return BadRequest("Invalid webhook signature.");
            }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var intent = stripeEvent.Data.Object as PaymentIntent;

                // ป้องกันซ้ำด้วย TransactionRef
                if (await _paymentRepo.FirstOrDefaultAsync(p => p.TransactionRef == intent.Id) != null)
                    return Ok(); // จัดการไปแล้ว

                // 🌟 ดึง Metadata ที่แนบไปตอน CreateIntent กลับมา
                if (!intent.Metadata.TryGetValue("BookingId", out var BookingIdStr) || !int.TryParse(BookingIdStr, out var BookingId))
                    return BadRequest("Invalid or missing BookingId in metadata");

                var Booking = await _BookingRepo.GetByIdAsync(BookingId, r => r.Include(r => r.Car));
                if (Booking == null)
                    return BadRequest("Booking not found");

                // 🌟 แก้ไข: เช็กจำนวนเงินกับ BookingPrice (ค่าจอง)
                var expectedAmount = (long)(Booking.Car.BookingPrice * 100);
                if (intent.AmountReceived != expectedAmount)
                    return BadRequest("Amount mismatch");

                // อัปเดตสถานะการจอง
                Booking.BookingStatus = SD.Booking_Confirmed;
                Booking.UpdatedAt = DateTime.UtcNow;
                await _BookingRepo.UpdateAsync(Booking);

                // บันทึก Payment ลงฐานข้อมูล
                var payment = new Payment
                {
                    BookingId = Booking.Id,
                    PaymentMethod = SD.PaymentMethod_CreditCard,
                    TotalPrice = Booking.Car.BookingPrice, // 🌟 แก้ไข: ใช้ BookingPrice
                    PaymentStatus = SD.Payment_Paid, // Stripe จ่ายผ่านแล้ว ถือว่าสถานะ Paid เลย
                    PaidAt = DateTime.UtcNow,
                    TransactionRef = intent.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _paymentRepo.AddAsync(payment);
            }

            return Ok();
        }

        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] string transactionRef)
        {
            var payment = await _paymentRepo.FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);
            if (payment == null || payment.PaymentStatus != SD.Payment_Paid)
                return BadRequest("ไม่พบรายการชำระเงิน หรือชำระเงินยังไม่สำเร็จ");

            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = transactionRef,
            };

            var refundService = new RefundService();
            await refundService.CreateAsync(refundOptions); // สั่ง Stripe คืนเงิน

            // อัปเดตสถานะในฐานข้อมูลของระบบเรา
            payment.PaymentStatus = SD.Payment_Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            
            // ยกเลิกการจองรถด้วยเมื่อคืนเงิน
            var booking = await _BookingRepo.GetByIdAsync(payment.BookingId, r => r.Include(r => r.Car));
            if (booking != null)
            {
                booking.BookingStatus = SD.Booking_Canceled;
                booking.CanceledAt = DateTime.UtcNow;
                if (booking.Car != null)
                {
                    booking.Car.CarStatus = SD.Status_Available; // คืนรถให้ระบบ
                }
                await _BookingRepo.UpdateAsync(booking);
            }

            await _paymentRepo.UpdateAsync(payment);

            return Ok("ทำการคืนเงินและยกเลิกการจองเรียบร้อย");
        }
    }
}