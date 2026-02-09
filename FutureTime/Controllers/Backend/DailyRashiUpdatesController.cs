using FutureTime.Helper;
using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using FutureTime.StaticData;
using Library.Data;
using Library.Exceptions;
using Library.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FutureTime.Controllers.Backend
{
    /// <summary>
    /// Controller for managing daily Rashi (horoscope) updates in the backend.
    /// Provides endpoints for creating, updating, retrieving, and listing daily horoscope updates.
    /// Only accessible by Admin and Support user types.
    /// </summary>
    [Route("backend/[controller]")]
    public class DailyRashiUpdatesController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyRashiUpdatesController"/> class.
        /// Ensures that only Admin and Support user types can access the controller.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor used to fill session details.</param>
        public DailyRashiUpdatesController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if (!new List<int> { 1, 2 }.Contains(request.user_type_id))//Only Admin & support
                throw new ErrorException("Not allowed");
        }

        
        /// <summary>
        /// Inserts a new daily Rashi (horoscope) update.
        /// Validates the input data, ensures all 12 rashi details are provided, and saves the update for the specified transaction date.
        /// Throws an error if the transaction date already exists or if the input is invalid.
        /// </summary>
        /// <param name="data">The daily horoscope update data to insert.</param>
        /// <returns>An <see cref="IActionResult"/> containing the operation result.</returns>
        [HttpPost]
        [Route("create")]
        public IActionResult Insert([FromBody] DailyHoroscopeUpdatesModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel);

                data._id = null;
                DateTime.TryParse(data.transaction_date, out DateTime _transaction_date);

                if (_transaction_date == DateTime.MinValue)
                {
                    throw new ErrorException("Enter valid date i.e yyyy-MM-dd.");
                }

                //Check if date already exists
                var filter = Builders<DailyHoroscopeUpdatesModel>.Filter.Eq("transaction_date", data.transaction_date);
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
                _ = MongoLogRecorder.RecordLogAsync<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel, data._id, request.user_id);
                response.message = "Daily Rashi Updates saved for the day.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        /// <summary>
        /// Loads the base data required for daily Rashi updates, such as the list of Rashis.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the base data for daily Rashi updates.</returns>
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

        
        /// <summary>
        /// Updates an existing daily Rashi (horoscope) update.
        /// Validates the input data, ensures all 12 rashi details are provided, and updates the update for the specified transaction date.
        /// Throws an error if the transaction date is invalid, the id is missing, or if the input is invalid.
        /// </summary>
        /// <param name="data">The daily horoscope update data to update.</param>
        /// <returns>An <see cref="IActionResult"/> containing the operation result.</returns>
        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> UpdateAsync([FromBody] DailyHoroscopeUpdatesModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel);

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
                var filter = Builders<DailyHoroscopeUpdatesModel>.Filter.Eq("_id", id);
                //var result = await col.UpdateOneAsync(filter,data.ToBsonDocument());

                var update = Builders<DailyHoroscopeUpdatesModel>.Update.Set("items", data.items).Set("updated_date", DateTime.Now).Set("updated_by", request.user_id);

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }

                _ = MongoLogRecorder.RecordLogAsync<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel, data._id, request.user_id);

                //col.InsertOne(data);
                response.message = "Daily Rashi Updates saved for the day.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        /// <summary>
        /// Retrieves the list of all daily Rashi (horoscope) updates.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of daily Rashi updates.</returns>
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAllList()
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel);
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

        
        /// <summary>
        /// Retrieves a specific daily Rashi (horoscope) update by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the daily Rashi update to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the requested daily Rashi update.</returns>
        [HttpGet]
        [Route("Get")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel);

                var obj_id = new ObjectId(id);

                var filter = Builders<DailyHoroscopeUpdatesModel>.Filter.Eq("_id", obj_id);
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
