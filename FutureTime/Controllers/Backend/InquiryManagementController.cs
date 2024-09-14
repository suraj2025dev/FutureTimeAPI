using FutureTime.Filters;
using Auth;
using Library;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using User;
using User.Data;
using static System.Net.WebRequestMethods;
using MongoDB.Driver;
using static Dapper.SqlMapper;
using FutureTime.MongoDB.Model;
using FutureTime.MongoDB;
using Library.Extensions;
using Library.Exceptions;
using FutureTime.StaticData;
using MongoDB.Bson;
using Microsoft.VisualBasic;
using FutureTime.MongoDB.Data;

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class InquiryManagementController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public InquiryManagementController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);

        }

        [HttpGet]
        [Route("GetInquiries")]
        public async Task<IActionResult> GetInquiries(string inquiry_state, string inquiry_status)
        {
            var _inquiry_status = inquiry_status == "pending" ? INQUIRY_STATUS.Pending : INQUIRY_STATUS.Completed;
            var _inquiry_state = GetEnumFromStatus(inquiry_status);
            try
            {
                //var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("inquiry_status", _inquiry_status),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("inquiry_state", _inquiry_state)
                                );


                var items = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel)
                                    .Find(filters).ToList()
                                    .Select(s => new
                                    {
                                        s.inquiry_regular.question,
                                        s.inquiry_regular.price,
                                        s.inquiry_number,
                                        payment_successfull = s.inquiry_payment_status == INQUIRY_PAYMENT_STATUS.Paid ? true : false,
                                        purchased_on = s.created_date,
                                        s.profile1,
                                        s.profile2,
                                        s.inquiry_regular.auspicious_from_date,
                                        s.inquiry_regular.category_type_id,
                                        is_replied = s.inquiry_state == INQUIRY_STATE.Published ? true : false,
                                        s.is_read,
                                        assignee = s.assignee_id,//TODO
                                    }).OrderByDescending(o => o.purchased_on).ToList();

                response.data.Add("list", items);
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpPost]
        [Route("ChangeAssignee")]
        public async Task<IActionResult> ChangeAssignee([FromBody] ChangeInquiryAssigneeDTO dto)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("_id", dto.inquiry_id)
                                );

                //var reading_activity = new InquiryReading
                //{
                //    assignee_id = dto.assignee_id,
                //    description = dto.comment,
                //    updated_on = DateTime.Now
                //};

                var assignee = GetUsers(dto.assignee_id);
                var new_state = INQUIRY_STATE.New;
                if (assignee.user_type_id == 3)
                {
                    new_state = INQUIRY_STATE.Expert;
                }else if(assignee.user_type_id == 4)
                {
                    new_state = INQUIRY_STATE.Translator;
                }
                else if (assignee.user_type_id == 5)
                {
                    new_state = INQUIRY_STATE.Reviewer;
                }

                var update = Builders<StartInquiryProcessModel>.Update
                    //.Push(i => i.inquiry_regular.reading_activity, reading_activity)  // Push to reading_activity array
                    .Set(i => i.inquiry_state, new_state)  // Update updated_date field
                    .Set(i => i.inquiry_status, INQUIRY_STATUS.Pending)
                    .Set(i => i.updated_by, request.user_id)
                    .Set(i => i.assignee_id, dto.assignee_id)
                    .Set(i => i.comment_for_assignee, dto.comment)
                    .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field


                //.Set(i=>i.inquiry_state = INQUIRY_STATE.Expert);

                var result = await col.UpdateOneAsync(filters, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel, dto.inquiry_id , request.user_id);

                response.message = "Inquiry Assigned.";

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpPost]
        [Route("PushComment")]
        public async Task<IActionResult> PushComment([FromBody] ChangeInquiryAssigneeDTO dto)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("_id", dto.inquiry_id)
                                );

                var reading_activity = new InquiryReading
                {
                    assignee_id = dto.assignee_id,
                    description = dto.comment,
                    updated_on = DateTime.Now
                };

                var update = Builders<StartInquiryProcessModel>.Update
                    .Push(i => i.inquiry_regular.reading_activity, reading_activity)  // Push to reading_activity array
                    .Set(i => i.inquiry_status, INQUIRY_STATUS.Completed)
                    .Set(i => i.updated_by, request.user_id)
                    .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field

                var result = await col.UpdateOneAsync(filters, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel, dto.inquiry_id, request.user_id);

                response.message = "Comment Pushed.";

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpPost]
        [Route("PublishInquiry")]
        public async Task<IActionResult> PublishInquiry([FromBody] ChangeInquiryAssigneeDTO dto)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("_id", dto.inquiry_id)
                                );

                var reading_activity = new InquiryReading
                {
                    assignee_id = dto.assignee_id,
                    description = dto.comment,
                    updated_on = DateTime.Now
                };

                var update = Builders<StartInquiryProcessModel>.Update
                    .Push(i => i.inquiry_regular.reading_activity, reading_activity)  // Push to reading_activity array
                    .Set(i => i.inquiry_status, INQUIRY_STATUS.Completed)
                    .Set(i => i.updated_by, request.user_id)
                    .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field

                var result = await col.UpdateOneAsync(filters, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel, dto.inquiry_id, request.user_id);

                response.message = "Comment Pushed.";

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpGet]
        [Route("GetAssigneeList")]
        public async Task<IActionResult> GetAssigneeList()
        {
            try
            {
                var user_type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.USER_TYPE).Select(s => s.list).First();

                response.data.Add("user_type", user_type);

                var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

                var items = await col.Find(new BsonDocument()).ToListAsync();

                response.data.Add("user", items);
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        private INQUIRY_STATE GetEnumFromStatus(string status)
        {
            if (Enum.TryParse(status, true, out INQUIRY_STATE result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException("Invalid status value");
            }
        }

        private UsersModel GetUsers(string user_id)
        {
            var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

            var obj_id = new ObjectId(user_id);

            var filter = Builders<UsersModel>.Filter.Eq("_id", obj_id);
            var item = col.Find(filter).FirstOrDefault();
            return item;
        }


    }
}
