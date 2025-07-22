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
using FutureTime.Helper;

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class UsersController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public UsersController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if(request.user_type_id != 1)//Only Admin
                throw new ErrorException("Not allowed");
        }

        
        [HttpPost]
        [Route("create")]
        public IActionResult Insert([FromBody] UsersModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

                data._id = null;
                
                //Check if email already exists.
                var filter = Builders<UsersModel>.Filter.Regex("email", Helper.Lib._BsonRegularExpression(data.email.ToLower(), "i"));
                var emailExists = col.Find(filter).Any();

                if (emailExists)
                {
                    throw new ErrorException("Email already exists.");
                }

                if(data.name == null || data.name == "")
                {
                    throw new ErrorException("Please provide name");
                }

                if (data.email == null || data.email == "")
                {
                    throw new ErrorException("Please provide email");
                }

                if (data.password == null || data.password == "")
                {
                    throw new ErrorException("Please provide password");
                }

                if (!(new List<int> { 1,2,3,4,5 }).Contains(data.user_type_id))
                {
                    throw new ErrorException("Please provide valid user type");
                }

                data.created_by = request.user_id;
                data.created_date = DateTime.Now;
                data.updated_by = request.user_id;
                data.updated_date = DateTime.Now;

                var result = col.InsertOneAsync(data);
                _ = MongoLogRecorder.RecordLogAsync<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel, data._id, request.user_id);
                response.message = "User created successfully.";
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
                var user_type = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.USER_TYPE).Select(s => s.list).First();
                response.data.Add("user_type", user_type);
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
        public async Task<IActionResult> UpdateAsync([FromBody] UsersModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

                if (data.name == null || data.name == "")
                {
                    throw new ErrorException("Please provide name");
                }

                if (data.email == null || data.email == "")
                {
                    throw new ErrorException("Please provide email");
                }

                if (!(new List<int> { 1, 2, 3, 4, 5 }).Contains(data.user_type_id))
                {
                    throw new ErrorException("Please provide valid user type");
                }

                var pw_changed = false;
                if (data.password != null && data.password != "")
                {
                    pw_changed = true;
                }

                #region Check Email Exists in other id
                var emailFilter = Builders<UsersModel>.Filter.Regex("email", Helper.Lib._BsonRegularExpression(data.email.ToLower(), "i"));
                var idFilter = Builders<UsersModel>.Filter.Ne("_id", data._id);
                var combinedFilter = Builders<UsersModel>.Filter.And(emailFilter, idFilter);
                var emailExists = col.Find(combinedFilter).Any();

                if (emailExists)
                {
                    throw new ErrorException("This email address is in use.");
                }
                #endregion


                var id = new ObjectId(data._id);

                //Check if date already exists
                var filter = Builders<UsersModel>.Filter.Eq("_id", id);

                UpdateDefinition<UsersModel> update;
                if (!pw_changed)
                {
                    update = Builders<UsersModel>.Update
                     .Set(u => u.name, data.name)
                     .Set(u => u.email, data.email)
                     .Set(u => u.active, data.active)
                     .Set(u => u.user_type_id, data.user_type_id)
                     .Set("updated_date", DateTime.Now)
                     .Set("updated_by", request.user_id);
                }
                else
                {
                    update = Builders<UsersModel>.Update
                     .Set(u => u.name, data.name)
                     .Set(u => u.email, data.email)
                     .Set(u => u.user_type_id, data.user_type_id)
                     .Set(u => u.active, data.active)
                     .Set(u=>u.password,data.password)
                     .Set("updated_date", DateTime.Now)
                     .Set("updated_by", request.user_id);
                }

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }

                _ = MongoLogRecorder.RecordLogAsync<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel, data._id, request.user_id);

                //col.InsertOne(data);
                response.message = "User updated successfuly.";
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
                var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);
                var all_users = await UsersHelper.GetAllUserAsync();

                var items = await col.Find(new BsonDocument()).ToListAsync();

                items.ForEach(f => {
                    f.updated_by = UsersHelper.GetUserName(all_users, f.updated_by);
                    f.created_by = UsersHelper.GetUserName(all_users, f.created_by);
                });


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
                var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);

                var obj_id = new ObjectId(id);

                var filter = Builders<UsersModel>.Filter.Eq("_id", obj_id);
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
