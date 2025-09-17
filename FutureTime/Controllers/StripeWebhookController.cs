using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using FutureTime.Service;
using Library.Data;
using Library.Exceptions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Stripe;

namespace FutureTime.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly string _webhookSecret;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(IConfiguration configuration, ILogger<StripeWebhookController> logger)
        {
            _webhookSecret = configuration["Stripe:WebhookSecret"];
            _logger = logger;
        }

        [Route("callback")]
        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            Console.WriteLine("Stripe Webhook called.");
            ApplicationResponse response = new ApplicationResponse();

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);

                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                {
                    Console.WriteLine("Stripe payment successful"); ;
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    // Handle the successful payment here

                    var inquiry_id = paymentIntent.Metadata["inquiry_id"];
                    var inquiry_number = paymentIntent.Metadata["inquiry_number"];

                    var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                    var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                        Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                        Builders<StartInquiryProcessModel>.Filter.Eq("_id", inquiry_id)
                                    );

                    var update = Builders<StartInquiryProcessModel>.Update
                        .Set(i => i.inquiry_payment_status, INQUIRY_PAYMENT_STATUS.Paid)
                        .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field

                    var result = await col.UpdateOneAsync(filters, update);

                    if (result.MatchedCount == 0)
                    {
                        throw new ErrorException("Failed to update payment status.");
                    }
                    _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, inquiry_id, "stripe");

                    var inq = col.Find(filters).FirstOrDefault();

                    var dict = new Dictionary<string, string>
                    {
                        { "inquiry_id", inq._id },
                        { "inquiry_number", inq.inquiry_number },
                        { "question", inq.inquiry_regular.question }
                    };

                    response.message = "Payment verified.";
                    await new FirebaseService().PushNotificationAsync("Payment", "Payment was successfully received.", dict, inq.guest_id);
                }
                else if (stripeEvent.Type == EventTypes.PaymentMethodAttached)
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    // Handle the successful attachment of a PaymentMethod
                }
                else if(stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
                {
                    Console.WriteLine("Stripe payment failed");
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    // Handle the successful payment here
                    var inquiry_id = paymentIntent.Metadata["inquiry_id"];
                    var inquiry_number = paymentIntent.Metadata["inquiry_number"];
                    var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);
                    var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                        Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                        Builders<StartInquiryProcessModel>.Filter.Eq("_id", inquiry_id)
                                    );
                    var update = Builders<StartInquiryProcessModel>.Update
                        .Set(i => i.inquiry_payment_status, INQUIRY_PAYMENT_STATUS.Failed)
                        .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field
                    var result = await col.UpdateOneAsync(filters, update);
                    if (result.MatchedCount == 0)
                    {
                        throw new ErrorException("Failed to update payment status.");
                    }
                    _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, inquiry_id, "stripe");
                    var inq = col.Find(filters).FirstOrDefault();
                    var dict = new Dictionary<string, string>
                    {
                        { "inquiry_id", inq._id },
                        { "inquiry_number", inq.inquiry_number },
                        { "question", inq.inquiry_regular.question }
                    };
                    response.message = "Payment failed.";
                    await new FirebaseService().PushNotificationAsync("Payment", "Payment has failed. Please try again.", dict, inq.guest_id);
                }
                else
                {
                    // Unhandled event type
                }
                return Ok(response);
            }
            catch (StripeException)
            {

                return BadRequest();
            }
        }
    }
}
