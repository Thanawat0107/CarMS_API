using CarMS_API.Data;
using CarMS_API.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace CarMS_API.Repositorys
{
    public class StripeRepository
    {
        public StripeRepository(IConfiguration config)
        {
            StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amountBaht, int reservationId, string currency = "thb")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amountBaht * 100),
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
            {
                { "reservationId", reservationId.ToString() }
            }
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }
    }
}
