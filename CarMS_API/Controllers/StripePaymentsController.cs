using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Repositorys;
using Microsoft.AspNetCore.Mvc;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Responsts;
using Stripe;
using Microsoft.EntityFrameworkCore;
using PaymentMethod = CarMS_API.Models.PaymentMethod;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripePaymentsController : ControllerBase
    {
        private readonly StripeRepository _stripe;
        private readonly IRepository<Reservation> _reservationRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly string _webhookSecret;

        public StripePaymentsController(StripeRepository stripe,
            IRepository<Reservation> reservationRepo,
             IRepository<Payment> paymentRepo,
            IConfiguration config)
        {
            _stripe = stripe;
            _reservationRepo = reservationRepo;
            _paymentRepo = paymentRepo;
            _webhookSecret = config["StripeSettings:WebhookSecret"];
        }

        // การจ่ายผ่าน Stripe
        [HttpPost("create-intent")]
        public async Task<IActionResult> CreateIntent([FromBody] PaymentIntentCreateDto dto)
        {
            var reservation = await _reservationRepo.GetByIdAsync(dto.ReservationId, r => r.Include(r => r.Car));

            if (reservation == null || reservation.Status != ReservationStatus.Pending)
                return BadRequest(ApiResponse<string>.Fail("ไม่พบการจอง หรือสถานะไม่ถูกต้อง"));

            var intent = await _stripe.CreatePaymentIntentAsync(reservation.Car.Price, reservation.Id);

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

                var reservationIdStr = intent.Metadata["reservationId"];
                if (!int.TryParse(reservationIdStr, out var reservationId))
                    return BadRequest("Invalid reservationId in metadata");

                var reservation = await _reservationRepo.GetByIdAsync(reservationId, r => r.Include(r => r.Car));
                if (reservation == null)
                    return BadRequest("Reservation not found");

                // เช็กจำนวนเงินต้องตรง
                var expectedAmount = (long)(reservation.Car.Price * 100);
                if (intent.AmountReceived != expectedAmount)
                    return BadRequest("Amount mismatch");

                // อัปเดตสถานะการจอง
                reservation.Status = ReservationStatus.Confirmed;
                reservation.UpdatedAt = DateTime.UtcNow;
                await _reservationRepo.UpdateAsync(reservation);

                // บันทึก Payment
                var payment = new Payment
                {
                    ReservationId = reservation.Id,
                    Method = PaymentMethod.CreditCard,
                    TotalPrice = reservation.Car.Price,
                    Status = PaymentStatus.Paid,
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
            if (payment == null || payment.Status != PaymentStatus.Paid)
                return BadRequest("ไม่พบรายการชำระเงิน หรือชำระเงินยังไม่สำเร็จ");

            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = transactionRef,
            };

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(refundOptions);

            // อัปเดตสถานะในฐานข้อมูล
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepo.UpdateAsync(payment);

            return Ok("ทำการคืนเงินเรียบร้อย");
        }
    }
}
