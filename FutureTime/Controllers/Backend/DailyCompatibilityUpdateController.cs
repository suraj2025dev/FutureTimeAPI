
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

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class DailyCompatibilityUpdateController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public DailyCompatibilityUpdateController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if (!new List<int> { 1, 2 }.Contains(request.user_type_id))//Only Admin & support
                throw new ErrorException("Not allowed");
        }

        
        [HttpPost]
        [Route("create")]
        public IActionResult Insert([FromBody] DailyCompatibilityUpdateModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyCompatibilityUpdateModel>(MongoDBService.COLLECTION_NAME.DailyCompatibilityUpdateModel);

                data._id = null;
                DateTime.TryParse(data.transaction_date, out DateTime _transaction_date);

                if (_transaction_date == DateTime.MinValue)
                {
                    throw new ErrorException("Enter valid date i.e yyyy-MM-dd.");
                }

                //Check if date already exists
                var filter = Builders<DailyCompatibilityUpdateModel>.Filter.Eq("transaction_date", data.transaction_date);
                var document = col.Find(filter).FirstOrDefault();

                if (document != null)
                {
                    throw new ErrorException("Entry of this transaction date is already saved. Please update data from list page.");
                }

                if (data.items == null || data.items.Count == 0)
                {
                    throw new ErrorException("Please provide rashi details.");
                }

                if (data.items.Where(w => new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.Contains(w.rashi_id)).GroupBy(g => g.rashi_id).Count() != 12)
                {

                    throw new ErrorException("Please provide details of all 12 rashi.");
                }
                data.created_by = request.user_id;
                data.created_date = DateTime.Now;
                data.updated_by = request.user_id;
                data.updated_date = DateTime.Now;

                var result = col.InsertOneAsync(data);
                _ = MongoLogRecorder.RecordLogAsync<DailyCompatibilityUpdateModel>(MongoDBService.COLLECTION_NAME.DailyCompatibilityUpdateModel, data._id, request.user_id);

                response.message = "Daily Auspicious Time Updates saved for the day.";
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
                var rashi = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.RASHI).Select(s => s.list).First();
                response.data.Add("rashi", rashi);
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
        public async Task<IActionResult> UpdateAsync([FromBody] DailyCompatibilityUpdateModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyCompatibilityUpdateModel>(MongoDBService.COLLECTION_NAME.DailyCompatibilityUpdateModel);

                //data.id = null;
                DateTime.TryParse(data.transaction_date, out DateTime _transaction_date);

                if (_transaction_date == DateTime.MinValue)
                {
                    throw new ErrorException("Enter valid date i.e yyyy-MM-dd.");
                }

                if (data._id == null)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }

                if (data.items == null || data.items.Count == 0)
                {
                    throw new ErrorException("Please provide rashi details.");
                }

                if (data.items.Where(w => new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.Contains(w.rashi_id)).GroupBy(g => g.rashi_id).Count() != 12)
                {

                    throw new ErrorException("Please provide details of all 12 rashi.");
                }

                var id = new ObjectId(data._id);

                //Check if date already exists
                var filter = Builders<DailyCompatibilityUpdateModel>.Filter.Eq("_id", id);
                //var result = await col.UpdateOneAsync(filter,data.ToBsonDocument());

                var update = Builders<DailyCompatibilityUpdateModel>.Update
                                    .Set("items", data.items)
                                    .Set("updated_date", DateTime.Now)
                                    .Set("updated_by", request.user_id);

                var result = await col.UpdateOneAsync(filter, update);
                _ = MongoLogRecorder.RecordLogAsync<DailyCompatibilityUpdateModel>(MongoDBService.COLLECTION_NAME.DailyCompatibilityUpdateModel, data._id, request.user_id);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }



                //col.InsertOne(data);
                response.message = "Daily Auspicious Time saved for the day.";
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
                var col = MongoDBService.ConnectCollection<DailyCompatibilityUpdateModel>(MongoDBService.COLLECTION_NAME.DailyCompatibilityUpdateModel);


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
                var col = MongoDBService.ConnectCollection<DailyCompatibilityUpdateModel>(MongoDBService.COLLECTION_NAME.DailyCompatibilityUpdateModel);

                var obj_id = new ObjectId(id);

                var filter = Builders<DailyCompatibilityUpdateModel>.Filter.Eq("_id", obj_id);
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
