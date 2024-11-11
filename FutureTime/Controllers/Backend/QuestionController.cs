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

        }

        
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> InsertAsync([FromBody] QuestionModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel);

                data._id = null;

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
                    question_category = s.category
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
                     .Set("updated_date", DateTime.Now)
                     .Set("updated_by", request.user_id);

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
        public async Task<IActionResult> GetAllList()
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel);


                var items = await col.Find(new BsonDocument()).ToListAsync();

                response.data.Add("list", items);
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
