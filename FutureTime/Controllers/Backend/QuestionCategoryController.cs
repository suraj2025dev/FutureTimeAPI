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
    public class QuestionCategoryController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public QuestionCategoryController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);

        }

        
        [HttpPost]
        [Route("create")]
        public IActionResult Insert([FromBody] QuestionCategoryModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);

                data._id = null;

                //Check if category already exists.
                #region Check Category Name Exists in other id
                var c_filter = Builders<QuestionCategoryModel>.Filter.Regex("category", new BsonRegularExpression(data.category.ToLower(), "i"));
                var ctFilter = Builders<QuestionCategoryModel>.Filter.Eq("category_type_id", data.category_type_id);
                var combinedFilter = Builders<QuestionCategoryModel>.Filter.And(c_filter, ctFilter);
                var exists = col.Find(combinedFilter).Any();

                if (exists)
                {
                    throw new ErrorException("This category description is already in use.");
                }
                #endregion

                if (data.category==null || data.category == "")
                {
                    throw new ErrorException("Please provide category.");
                }

                var category_type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.CATEGORY_TYPE).Select(s => s.list).First();
                if (!category_type.Select(s => s.id).ToList().Contains(data.category_type_id.ToString()))
                {
                    throw new ErrorException("Undefined category type.");
                }
                data.created_by = request.user_id;
                data.created_date = DateTime.Now;
                data.updated_by = request.user_id;
                data.updated_date = DateTime.Now;

                var result = col.InsertOneAsync(data);
                _ = MongoLogRecorder.RecordLogAsync<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel, data._id, request.user_id);

                response.message = "Category created successfully.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        [HttpGet]
        [Route("loadbasedata")]
        public IActionResult LoadBaseData()
        {
            try
            {
                var type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.CATEGORY_TYPE).Select(s => s.list).First();
                response.data.Add("category_type", type);
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
        public async Task<IActionResult> UpdateAsync([FromBody] QuestionCategoryModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);


                #region Check Category Name Exists in other id
                var c_filter = Builders<QuestionCategoryModel>.Filter.Regex("category", new BsonRegularExpression(data.category.ToLower(), "i"));
                var idFilter = Builders<QuestionCategoryModel>.Filter.Ne("_id", data._id);
                var ctFilter = Builders<QuestionCategoryModel>.Filter.Ne("category_type_id", data.category_type_id);
                var combinedFilter = Builders<QuestionCategoryModel>.Filter.And(c_filter, idFilter, ctFilter);
                var exists = col.Find(combinedFilter).Any();

                if (exists)
                {
                    throw new ErrorException("This category name is already in use.");
                }
                #endregion

                if (data.category == null || data.category == "")
                {
                    throw new ErrorException("Please provide category.");
                }


                var category_type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.CATEGORY_TYPE).Select(s => s.list).First();
                if (!category_type.Select(s => s.id).ToList().Contains(data.category_type_id.ToString()))
                {
                    throw new ErrorException("Undefined category type.");
                }

                var id = new ObjectId(data._id);

                //Check if date already exists
                var filter = Builders<QuestionCategoryModel>.Filter.Eq("_id", id);

                var update = Builders<QuestionCategoryModel>.Update
                     .Set(u => u.category_type_id, data.category_type_id)
                     .Set(u => u.category, data.category)
                     .Set(u => u.active, data.active)
                     .Set(u => u.order_id, data.order_id)
                     .Set(u => u.updated_by, request.user_id)
                     .Set(u => u.updated_date, DateTime.Now);

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }

                _ = MongoLogRecorder.RecordLogAsync<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel, data._id, request.user_id);



                //col.InsertOne(data);
                response.message = "Question Category updated successfuly.";
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
                var col = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);


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
                var col = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);

                var obj_id = new ObjectId(id);

                var filter = Builders<QuestionCategoryModel>.Filter.Eq("_id", obj_id);
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
