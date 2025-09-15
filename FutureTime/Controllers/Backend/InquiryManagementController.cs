using FutureTime.Helper;
using FutureTime.MongoDB;
using FutureTime.MongoDB.Data;
using FutureTime.MongoDB.Model;
using FutureTime.Service;
using FutureTime.StaticData;
using Library.Data;
using Library.Exceptions;
using Library.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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
            //request.user_id = "123";
        }

        [HttpGet]
        [Route("GetInquiries")]

        public async Task<IActionResult> GetInquiries(
            string inquiry_state = null, 
            string inquiry_status = null,
            string inquiry_number = null,
            string inquiry_date = null,//YYYY-MM-DD
            int? inquiry_payment_status = null,//0: pending, 1: paid, 2:failed
            int? category_type_id = null,
            string question_id = null,
            string assignee_id = null,
            int page_number = 1,
            int page_size = 100
            )
        {

            int skip = (page_number - 1) * page_size;

            try
            {
                //var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true)
                                );

                if(inquiry_state != null)
                {
                    var _inquiry_state = GetEnumFromStatus(inquiry_state);
                    filters = filters & Builders<StartInquiryProcessModel>.Filter.Eq("inquiry_state", _inquiry_state);
                }

                if (inquiry_status != null)
                {
                    var _inquiry_status = inquiry_status == "pending" ? INQUIRY_STATUS.Pending : INQUIRY_STATUS.Completed;
                    filters = filters & Builders<StartInquiryProcessModel>.Filter.Eq("inquiry_status", _inquiry_status);
                }

                if (inquiry_number != null)
                {
                    filters = filters & Builders<StartInquiryProcessModel>.Filter.Eq("inquiry_number", inquiry_number);
                }

                if (inquiry_date != null)
                {
                    DateTime.TryParse(inquiry_date, out DateTime _transaction_date);

                    if (_transaction_date == DateTime.MinValue)
                    {
                        throw new ErrorException("Enter valid date i.e yyyy-MM-dd.");
                    }

                    DateTime startOfDay = _transaction_date.Date; // Midnight (00:00:00)
                    DateTime endOfDay = startOfDay.AddDays(1).AddTicks(-1); // Last moment of the day (23:59:59.999)

                    filters = filters & Builders<StartInquiryProcessModel>.Filter.Gte("created_date", startOfDay) &
                                        Builders<StartInquiryProcessModel>.Filter.Lte("created_date", endOfDay);
                }

                if (inquiry_payment_status != null)
                {
                    var _inquiry_payment_status = (INQUIRY_PAYMENT_STATUS)Enum.ToObject(typeof(INQUIRY_PAYMENT_STATUS), inquiry_payment_status);

                    filters = filters & Builders<StartInquiryProcessModel>.Filter.Eq("inquiry_payment_status", _inquiry_payment_status);
                }

                if (category_type_id != null)
                {
                   filters = filters & Builders<StartInquiryProcessModel>.Filter.Eq(x => x.inquiry_regular.category_type_id, category_type_id);
                }

                if (question_id != null)
                {
                    filters = filters & Builders<StartInquiryProcessModel>.Filter.Eq(x => x.inquiry_regular.question_id, question_id);
                }

                if (assignee_id != null)
                {
                    filters = filters & Builders<StartInquiryProcessModel>.Filter.Eq(x => x.assignee_id, assignee_id);
                }

                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);
                var items = col
                            .Find(filters)
                            .ToList()
                            .OrderByDescending(o => o.updated_date)
                            .Skip(skip)
                            .Take(page_size)
                            .ToList()
                            .Select(s => new
                            {
                                inquiry_id = s._id,
                                s.inquiry_regular.question,
                                s.inquiry_regular.price,
                                s.inquiry_number,
                                payment_successfull = s.inquiry_payment_status == INQUIRY_PAYMENT_STATUS.Paid ? true : false,
                                inquiry_status = Enum.GetName(typeof(INQUIRY_STATUS), s.inquiry_status),
                                inquiry_state = Enum.GetName(typeof(INQUIRY_STATE), s.inquiry_state),
                                purchased_on = s.created_date,
                                s.profile1,
                                s.profile2,
                                s.inquiry_regular.auspicious_from_date,
                                s.inquiry_regular.horoscope_from_date,
                                s.inquiry_regular.category_type_id,
                                //reading_activity=s.inquiry_regular.reading_activity.Select(s=>new List<InquiryReading>() { }),
                                //is_replied = s.inquiry_state == INQUIRY_STATE.Published ? true : false,
                                //s.is_read,
                                assignee = GetUsers(s.assignee_id)==null?"":GetUsers(s.assignee_id).name,//TODO
                                s.comment_for_assignee,
                                s.final_reading,
                                s.created_date,
                                //s.created_by,
                                //s.updated_by,
                                s.updated_date,
                                s.vedic_api_response_list
                            }).ToList();
                var totalCount = await col.CountDocumentsAsync(filters);
                response.data.Add("list", items);
                response.data.Add("total_count", totalCount);
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

                //var assigned_person = col.Find(filters).FirstOrDefault().ass;

                var assignee = GetUsers(dto.assignee_id);
                if (assignee == null)
                    throw new ErrorException("Invalid assignee.");
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
                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, dto.inquiry_id , request.user_id);

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
                var item = col.Find(filters).FirstOrDefault();

                if(item.inquiry_status == INQUIRY_STATUS.Completed)
                {
                    throw new ErrorException("Can not post comment in completed inquiry");
                }

                if(col.Find(filters).FirstOrDefault().assignee_id != request.user_id)
                {
                    throw new ErrorException("Assigned person must push the comment.");
                }

                var reading_activity = new InquiryReading
                {
                    assignee_id = col.Find(filters).FirstOrDefault().assignee_id,
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
                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, dto.inquiry_id, request.user_id);

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
        [Route("GetCommentHistory")]
        
        public async Task<IActionResult> GetCommentHistory(string inquiry_id)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("_id", inquiry_id)
                                );

                var item = col.Find(filters).FirstOrDefault();

                var comment = item.inquiry_regular.reading_activity.Select(s => new { 
                    assignee = GetUsers(s.assignee_id) == null ? "" : GetUsers(s.assignee_id).name,
                    s.description,
                    s.updated_on
                }).OrderByDescending(o=>o.updated_on).ToList();

                response.data.Add("comment", comment);

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

                //var reading_activity = new InquiryReading
                //{
                //    assignee_id = dto.assignee_id,
                //    description = dto.comment,
                //    updated_on = DateTime.Now
                //};

                var update = Builders<StartInquiryProcessModel>.Update
                    //.Push(i => i.inquiry_regular.reading_activity, reading_activity)  // Push to reading_activity array
                    .Set(i => i.final_reading, dto.comment)
                    .Set(i => i.inquiry_status, INQUIRY_STATUS.Completed)
                    .Set(i => i.inquiry_state, INQUIRY_STATE.Published)
                    .Set(i => i.updated_by, request.user_id)
                    .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field

                var result = await col.UpdateOneAsync(filters, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, dto.inquiry_id, request.user_id);

                var inq = col.Find(filters).FirstOrDefault();

                var dict = new Dictionary<string, string>();
                dict.Add("inquiry_id", inq._id);
                dict.Add("inquiry_number", inq.inquiry_number);
                dict.Add("question", inq.inquiry_regular.question);
                await new FirebaseService().PushNotificationAsync("Inquiry", "You got a reply.", dict, inq.guest_id);

                response.message = "Published to the user.";

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpPost]
        [Route("RejectInquiry")]

        public async Task<IActionResult> RejectInquiry([FromBody] ChangeInquiryAssigneeDTO dto)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("_id", dto.inquiry_id)
                                );

                var inq = col.Find(filters).FirstOrDefault();
                if(inq.inquiry_state == INQUIRY_STATE.Published)
                {
                    throw new ErrorException("Sorry, published inquiry can not be rejected.");
                }

                var update = Builders<StartInquiryProcessModel>.Update
                    .Set(i => i.inquiry_status, INQUIRY_STATUS.Pending)
                    .Set(i => i.inquiry_state, INQUIRY_STATE.Rejected)
                    .Set(i => i.updated_by, request.user_id)
                    .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field

                var result = await col.UpdateOneAsync(filters, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, dto.inquiry_id, request.user_id);


                response.message = "Rejected an inquiry.";

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
                var user_type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.USER_TYPE).Select(s => s.list).First().Where(w=>int.Parse(w.id) >=3).ToList();

                response.data.Add("user_type", user_type);

                var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

                var items = await col.Find(new BsonDocument()).ToListAsync();

                response.data.Add("user", items.Where(w=>w.user_type_id>=3).ToList());
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpGet]
        [Route("GetInquiriyByNumber")]
        
        public async Task<IActionResult> GetInquiriyByNumber(string inquiry_number)
        { 
            try
            {
                
                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("inquiry_number", inquiry_number)
                                );


                var items = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel)
                                    .Find(filters).ToList()
                                    .Select(s => new
                                    {
                                        inquiry_id = s._id,
                                        s.inquiry_regular.question,
                                        s.inquiry_regular.price,
                                        s.inquiry_number,
                                        payment_successfull = s.inquiry_payment_status == INQUIRY_PAYMENT_STATUS.Paid ? true : false,
                                        purchased_on = s.created_date,
                                        s.profile1,
                                        s.profile2,
                                        s.inquiry_regular.auspicious_from_date,
                                        s.inquiry_regular.horoscope_from_date,
                                        s.inquiry_regular.category_type_id,
                                        //reading_activity=s.inquiry_regular.reading_activity.Select(s=>new List<InquiryReading>() { }),
                                        //is_replied = s.inquiry_state == INQUIRY_STATE.Published ? true : false,
                                        //s.is_read,
                                        assignee = GetUsers(s.assignee_id) == null ? "" : GetUsers(s.assignee_id).name,//TODO
                                        s.comment_for_assignee,
                                        s.final_reading,
                                        s.vedic_api_response_list
                                    }).OrderByDescending(o => o.purchased_on).ToList();

                if(items.Count() == 0)
                {
                    throw new ErrorException("Inquiry not found.");
                }

                response.data.Add("inquiry", items[0]);
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpGet]
        [Route("GetFilterForInquiry")]

        public async Task<IActionResult> GetFilterForInquiry()
        {
            try
            {
                var category_type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.CATEGORY_TYPE).Select(s => s.list).First().ToList();

                response.data.Add("category_type", category_type);

                var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

                var items = await col.Find(new BsonDocument()).ToListAsync();

                response.data.Add("assignee_list", items.Where(w => w.user_type_id >= 3).Select(s => new { 
                    s._id,
                    s.name,
                    s.user_type_id
                }).ToList());

                #region GetQuestionWithCat
                var pipeline = new[]
                 {
                    new BsonDocument
                    {
                        { "$addFields", new BsonDocument
                            {
                                { "question_category_id", new BsonDocument("$toObjectId", "$question_category_id") }
                            }
                        }
                    },
                    new BsonDocument
                    {
                        { "$lookup", new BsonDocument
                            {
                                { "from", "QuestionCategoryModel" }, // Target collection name
                                { "localField", "question_category_id" }, // Field in QuestionModel
                                { "foreignField", "_id" }, // Field in QuestionCategoryModel
                                { "as", "category_details" } // Result field
                            }
                        }
                    },
                    new BsonDocument
                    {
                        { "$unwind", new BsonDocument
                            {
                                { "path", "$category_details" },
                                { "preserveNullAndEmptyArrays", true } // Allow questions without categories
                            }
                        }
                    },
                    new BsonDocument
                    {
                        { "$project", new BsonDocument
                            {
                                { "_id", 1 },
                                { "question", 1 },
                                { "question_category_id", 1 },
                                { "category_name", "$category_details.category" }, // Include category name
                                { "category_type_id", "$category_details.category_type_id" } // Include category name
                            }
                        }
                    }
                };

                var result = await MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel).Aggregate<BsonDocument>(pipeline).ToListAsync();
                var mappedResult = result.Select(doc => new
                {
                    _id = doc["_id"].AsObjectId.ToString(),
                    question = doc["question"].AsString,
                    question_category_id = doc["question_category_id"].AsObjectId.ToString(),
                    question_category_name = doc.GetValue("category_name", BsonNull.Value)?.AsString,
                    category_type_id = doc.GetValue("category_type_id", BsonNull.Value)?.AsInt32,
                    category_type = FTStaticData.GetName(STATIC_DATA_TYPE.CATEGORY_TYPE, (doc.GetValue("category_type_id", BsonNull.Value)?.AsInt32).ToString())
                }).ToList();
                response.data.Add("question", mappedResult);


                #endregion
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpGet]
        [Route("CallVedicAPI")]

        public async Task<IActionResult> CallVedicAPI(string inquiry_id, string vedic_api_type_id)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("_id", inquiry_id)
                                );
                var item = col.Find(filters).FirstOrDefault();

                var vedic_api_response = item.vedic_api_response_list;

                if(vedic_api_response == null)
                {
                    vedic_api_response = new List<VedicAPIResponse>();
                }


                if (vedic_api_type_id == "1")
                {
                    var a= await VedicAPIConnection.APICall.GetPlanetDetail(
                            DateTime.Now,
                            item.profile1.tob,
                            item.profile1.city.lat,
                            item.profile1.city.lng,
                            item.profile1.tz.ToString()
                        );

                    
                    var index = vedic_api_response.FindIndex(f => f.vedic_api_type_id == vedic_api_type_id);
                    if (index > -1)
                    {
                        vedic_api_response[index] = new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        };
                    }
                    else
                    {
                        vedic_api_response.Add(new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        });
                    }
                }
                else if (vedic_api_type_id == "2")
                {
                    var a = await VedicAPIConnection.APICall.GetPlanetDetail(
                            DateTime.Parse(item.profile1.dob),
                            item.profile1.tob,
                            item.profile1.city.lat,
                            item.profile1.city.lng,
                            item.profile1.tz.ToString()
                        );


                    var index = vedic_api_response.FindIndex(f => f.vedic_api_type_id == vedic_api_type_id);
                    if (index > -1)
                    {
                        vedic_api_response[index] = new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        };
                    }
                    else
                    {
                        vedic_api_response.Add(new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        });
                    }
                }
                else if (vedic_api_type_id == "3")
                {
                    var a = await VedicAPIConnection.APICall.MatchMaking(
                             DateTime.Parse(item.profile1.dob),
                             item.profile1.tob,
                             item.profile1.city.lat,
                             item.profile1.city.lng,
                             item.profile1.tz.ToString(),

                             DateTime.Parse(item.profile2.dob),
                             item.profile2.tob,
                             item.profile2.city.lat,
                             item.profile2.city.lng,
                             item.profile2.tz.ToString()
                         );


                    var index = vedic_api_response.FindIndex(f => f.vedic_api_type_id == vedic_api_type_id);
                    if (index > -1)
                    {
                        vedic_api_response[index] = new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        };
                    }
                    else
                    {
                        vedic_api_response.Add(new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        });
                    }
                }
                else if (vedic_api_type_id == "4")
                {
                    var a = await VedicAPIConnection.APICall.GetPanchang(//Panchang for either horoscope from date or aus from date
                           (DateTime)item.inquiry_regular.auspicious_from_date == null ? (DateTime)item.inquiry_regular.horoscope_from_date : (DateTime)item.inquiry_regular.auspicious_from_date,
                           item.profile1.tob,
                           item.profile1.city.lat,
                           item.profile1.city.lng,
                           item.profile1.tz.ToString()
                       );


                    var index = vedic_api_response.FindIndex(f => f.vedic_api_type_id == vedic_api_type_id);
                    if (index > -1)
                    {
                        vedic_api_response[index] = new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        };
                    }
                    else
                    {
                        vedic_api_response.Add(new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        });
                    }
                }
                else if (vedic_api_type_id == "5")
                {
                    var a = await VedicAPIConnection.APICall.GetMahadasha(
                           (DateTime)item.inquiry_regular.auspicious_from_date == null ? (DateTime)item.inquiry_regular.horoscope_from_date : (DateTime)item.inquiry_regular.auspicious_from_date,
                           item.profile1.tob,
                           item.profile1.city.lat,
                           item.profile1.city.lng,
                           item.profile1.tz.ToString()
                       );


                    var index = vedic_api_response.FindIndex(f => f.vedic_api_type_id == vedic_api_type_id);
                    if (index > -1)
                    {
                        vedic_api_response[index] = new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        };
                    }
                    else
                    {
                        vedic_api_response.Add(new VedicAPIResponse
                        {
                            vedic_api_type_id = vedic_api_type_id,
                            vedic_api_response = a.ToString()
                        });
                    }
                }

                var update = Builders<StartInquiryProcessModel>.Update
                    .Set(i => i.vedic_api_response_list, vedic_api_response)
                    .Set(i => i.updated_by, request.user_id)
                    .Set(i => i.updated_date, DateTime.Now);  // Update updated_date field

                var result = await col.UpdateOneAsync(filters, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, inquiry_id, request.user_id);

                response.message = "Vedic API Fetched.";

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [HttpGet]
        [Route("GetVedicAPIType")]
        public IActionResult GetVedicAPIType()
        {
            try
            {
                var vedic_api_type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.VEDIC_API_TYPE).Select(s => s.list).First();
                response.data.Add("vedic_api_type", vedic_api_type);
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
            if (user_id == null) return null;
            var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

            var obj_id = new ObjectId(user_id);

            var filter = Builders<UsersModel>.Filter.Eq("_id", obj_id);
            var item = col.Find(filter).FirstOrDefault();
            return item;
        }


    }
}
