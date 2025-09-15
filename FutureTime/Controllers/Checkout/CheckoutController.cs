using Auth;
using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using Library.Data;
using Library.Exceptions;
using Library.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FutureTime.Controllers.Backend
{
    [Route("[controller]")]
    public class CheckoutController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public CheckoutController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillGuestSessionAsync(request);

        }

        [AnonymousAuthorizeFilter]
        [HttpGet("success/{auth}/{inquiry_id}")]
        public async Task<IActionResult> success(string auth, string inquiry_id)
        {
            if (AppStatic.CONFIG.App.Stripe.StripeWebHookToken != auth)
            {
                return StatusCode(401);
            }

            try
            {
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
                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, inquiry_id, request.user_id);

                response.message = "Payment verified.";

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            return Ok(response);
        }

        [AnonymousAuthorizeFilter]
        [HttpGet("cancel/{auth}/{inquiry_id}")]
        public async Task<IActionResult> cancel(string auth, string inquiry_id)
        {
            if (AppStatic.CONFIG.App.Stripe.StripeWebHookToken != auth)
            {
                return StatusCode(401);
            }

            try
            {
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
                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, inquiry_id, request.user_id);

                response.message = "Payment cancelled.";

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            return Ok(response);
        }


    }
}
