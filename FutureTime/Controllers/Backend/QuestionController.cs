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
using FutureTime.Helper;
using FutureTime.MongoDB.Data;
using MongoDB.Bson.Serialization;

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class QuestionController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public QuestionController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if (!new List<int> { 1,2}.Contains(request.user_type_id))//Only Admin & support
                throw new ErrorException("Not allowed");
        }

        
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> InsertAsync([FromBody] QuestionModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel);

                data._id = null;

                if (data.effective_from != null)
                {
                    if (!DateTime.TryParse(data.effective_from, out DateTime _eff))
                    {
                        throw new ErrorException("Please provide valid effective from in format like 2024-01-01");
                    }
                }

                if (data.effective_to != null)
                {
                    if (!DateTime.TryParse(data.effective_to, out DateTime _eff))
                    {
                        throw new ErrorException("Please provide valid effective to in format like 2024-01-01");
                    }
                }

                if (data.discount_amount > 0)
                {
                    data.price = data.price_before_discount - data.discount_amount;
                    if (data.price < 0)
                    {
                        throw new ErrorException("Discount too high.");
                    }
                }

                //Check if question already exists.
                //var filter = Builders<QuestionModel>.Filter.Regex("question", new BsonRegularExpression(data.question.ToLower(), "i"));

                var filter = Builders<QuestionModel>.Filter.And(
                                Builders<QuestionModel>.Filter.Regex("question", new BsonRegularExpression(data.question.ToLower(), "i")),
                                Builders<QuestionModel>.Filter.Regex("question_category_id", new BsonRegularExpression($"^{data.question_category_id}$", "i"))
                            );

                var exists = col.Find(filter).Any();

                if (exists)
                {
                    throw new ErrorException("Question with same description already exists.");
                }

                if(data.question == null || data.question == "")
                {
                    throw new ErrorException("Please provide question.");
                }

                //Validate category
                var category_list = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);

                var category_items = await category_list.Find(new BsonDocument()).ToListAsync();
                if (!category_items.Select(s => s._id).ToList().Contains(data.question_category_id))
                {
                    throw new ErrorException("Please choose valid question category.");
                }

                if (data.price == null || data.price < 0)
                {
                    throw new ErrorException("Please enter valid price.");
                }

                data.created_by = request.user_id;
                data.created_date = DateTime.Now;
                data.updated_by = request.user_id;
                data.updated_date = DateTime.Now;

                var result = col.InsertOneAsync(data);
                _ = MongoLogRecorder.RecordLogAsync<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel, data._id, request.user_id);
                response.message = "Question created successfully.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        [HttpGet]
        [Route("loadbasedata")]
        public async Task<IActionResult> LoadBaseDataAsync()
        {
            try
            {
                //Validate category
                var category_list = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);

                var category_items = await category_list.Find(new BsonDocument()).ToListAsync();
                response.data.Add("question_category_items", category_items.ToList().Select(s=>new { 
                    question_category_id = s._id,
                    question_category = FTStaticData.GetName(STATIC_DATA_TYPE.CATEGORY_TYPE,s.category_type_id.ToString())+" : "+s.category
                }).ToList());
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        
        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> UpdateAsync([FromBody] QuestionModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel);

                if (data.effective_from != null)
                {
                    if (!DateTime.TryParse(data.effective_from, out DateTime _eff))
                    {
                        throw new ErrorException("Please provide valid effective from in format like 2024-01-01");
                    }
                }

                if (data.effective_to != null)
                {
                    if (!DateTime.TryParse(data.effective_to, out DateTime _eff))
                    {
                        throw new ErrorException("Please provide valid effective to in format like 2024-01-01");
                    }
                }

                #region Check question Name Exists in other id
                var c_filter = Builders<QuestionModel>.Filter.Regex("question", new BsonRegularExpression(data.question.ToLower(), "i"));
                var idFilter = Builders<QuestionModel>.Filter.Ne("_id", data._id);
                var qcFilter = Builders<QuestionModel>.Filter.Eq("question_category_id", data.question_category_id);
                var combinedFilter = Builders<QuestionModel>.Filter.And(c_filter, idFilter, qcFilter);
                var exists = col.Find(combinedFilter).Any();

                if (exists)
                {
                    throw new ErrorException("Question with same description already exists.");
                }
                #endregion



                if (data.question == null || data.question == "")
                {
                    throw new ErrorException("Please provide question.");
                }

                //Validate category
                var category_list = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);

                var category_items = await category_list.Find(new BsonDocument()).ToListAsync();
                if (!category_items.Select(s => s._id).ToList().Contains(data.question_category_id))
                {
                    throw new ErrorException("Please choose valid question category.");
                }

                if(data.price == null || data.price < 0)
                {
                    throw new ErrorException("Please enter valid price.");
                }

                var id = new ObjectId(data._id);

                //Check if date already exists
                var filter = Builders<QuestionModel>.Filter.Eq("_id", id);

                var update = Builders<QuestionModel>.Update
                     .Set(u => u.question, data.question)
                     .Set(u => u.question_category_id, data.question_category_id)
                     .Set(u => u.active, data.active)
                     .Set(u => u.price, data.price)
                     .Set(u => u.order_id, data.order_id)
                     .Set(u => u.price_before_discount, data.price_before_discount)
                     .Set(u => u.is_initial_offerings, data.is_initial_offerings)
                     .Set(u => u.is_bundle, data.is_bundle)
                     .Set(u => u.effective_from, data.effective_from)
                     .Set(u => u.effective_to, data.effective_to)
                     .Set(u => u.discount_amount, data.discount_amount)
                     .Set("updated_date", DateTime.Now)
                     .Set("updated_by", request.user_id);

                if(data.image_blob!=null && data.image_blob != "")
                {
                    update.Set(u => u.image_blob, data.image_blob);
                }

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel, data._id, request.user_id);


                //col.InsertOne(data);
                response.message = "Question updated successfuly.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAllList(GetAllQuestionsData data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel);
                var all_users = await UsersHelper.GetAllUserAsync();

                if (data.page_number == null)
                    data.page_number = 1;

                if (data.page_size == null)
                    data.page_size = 10;

                int skip = ((int)data.page_number - 1) * (int)data.page_size;

                var filters = new List<FilterDefinition<QuestionModel>>();
                if (!string.IsNullOrEmpty(data.question))
                {
                    filters.Add(Builders<QuestionModel>.Filter.Regex("question", new BsonRegularExpression(data.question.ToLower(), "i")));
                }
                if (data.question_category_id != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.question_category_id,   data.question_category_id));
                }
                if (data.active != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.active, data.active));
                }

                if (data.price != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.price, data.price));
                }
                if (data.price_before_discount != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.price_before_discount, data.price_before_discount));
                }
                if (data.is_initial_offerings != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.is_initial_offerings, data.is_initial_offerings));
                }
                if (data.is_bundle != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.is_bundle, data.is_bundle));
                }
                if (data.effective_from != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.effective_from, data.effective_from));
                }
                if (data.effective_to != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.effective_to, data.effective_to));
                }
                if (data.discount_amount != null)
                {
                    filters.Add(Builders<QuestionModel>.Filter.Eq(doc => doc.discount_amount, data.discount_amount));
                }


                var combinedFilter = filters.Count > 0 ? Builders<QuestionModel>.Filter.And(filters) : Builders<QuestionModel>.Filter.Empty;

                var matchStage = new BsonDocument { { "$match", combinedFilter.Render(BsonSerializer.SerializerRegistry.GetSerializer<QuestionModel>(), BsonSerializer.SerializerRegistry) } };


                var pipeline = new[]
                {
                    matchStage,
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
                                { "updated_date", 1 },
                                { "updated_by", 1 },
                                { "created_date", 1 },
                                { "created_by", 1 },
                                { "question", 1 },
                                { "order_id", 1 },
                                { "question_category_id", 1 },
                                { "active", 1 },
                                { "price", 1 },
                                { "price_before_discount", 1 },
                                { "is_initial_offerings", 1 },
                                { "is_bundle", 1 },
                                { "image_blob", 1 },
                                { "effective_from", 1 },
                                { "effective_to", 1 },
                                { "discount_amount", 1 },
                                { "category_name", "$category_details.category" } // Include category name
                            }
                        }
                    },
                    new BsonDocument { { "$skip", skip } },  // Apply pagination
                    new BsonDocument { { "$limit", (int)data.page_size } }  // Apply pagination
                };

                var result = await col.Aggregate<BsonDocument>(pipeline).ToListAsync();
                var mappedResult = result.Select(doc => new
                {
                    _id = doc["_id"].AsObjectId.ToString(),
                    updated_date = doc["updated_date"].ToUniversalTime(),
                    updated_by = UsersHelper.GetUserName(all_users, doc["updated_by"].AsString),
                    created_date = doc["created_date"].ToUniversalTime(),
                    created_by = UsersHelper.GetUserName(all_users, doc["created_by"].AsString),
                    question = doc["question"].AsString,
                    order_id = doc["order_id"].AsInt32,
                    question_category_id = doc["question_category_id"].AsObjectId.ToString(),
                    active = doc["active"].AsBoolean,
                    price = Convert.ToDecimal(doc["price"].AsString),
                    question_category_name = doc.GetValue("category_name", BsonNull.Value)?.AsString,
                    price_before_discount= Convert.ToDecimal(doc["price_before_discount"].AsString),
                    is_initial_offerings = (doc["is_initial_offerings"].AsBoolean),
                    is_bundle = (doc["is_bundle"].AsBoolean),
                    image_blob = doc.GetValue("image_blob", BsonNull.Value).IsBsonNull ? (string?)null : doc.GetValue("image_blob", BsonNull.Value)?.AsString,
                    effective_from = doc.GetValue("effective_from", BsonNull.Value).IsBsonNull ? (string?)null : doc.GetValue("effective_from", BsonNull.Value)?.AsString,
                    effective_to = doc.GetValue("effective_to", BsonNull.Value).IsBsonNull ? (string?)null : doc.GetValue("effective_to", BsonNull.Value)?.AsString,
                    discount_amount = Convert.ToDecimal(doc["discount_amount"].AsString),
                }).ToList();

                var countPipeline = new[]
                {
                    matchStage,  // Apply the same filter from the main pipeline
                    new BsonDocument { { "$count", "total_count" } } // Count matching documents
                };

                // Run the aggregation
                var countResult = await col.Aggregate<BsonDocument>(countPipeline).FirstOrDefaultAsync();
                int totalCount = countResult != null ? countResult["total_count"].AsInt32 : 0;

                response.data.Add("total_count", totalCount);
                response.data.Add("list", mappedResult);

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        
        [HttpGet]
        [Route("Get")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel);

                var obj_id = new ObjectId(id);

                var filter = Builders<QuestionModel>.Filter.Eq("_id", obj_id);
                var item = await col.Find(filter).FirstOrDefaultAsync();

                response.data.Add("item", item);
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

    }
}
