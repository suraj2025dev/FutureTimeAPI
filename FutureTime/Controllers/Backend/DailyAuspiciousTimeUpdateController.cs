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
    /// Controller for managing daily auspicious time updates, including creation, update, retrieval, and base data loading.
    /// Only accessible by Admin and Support user types.
    /// </summary>
    [Route("backend/[controller]")]
    public class DailyAuspiciousTimeUpdateController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyAuspiciousTimeUpdateController"/> class.
        /// Sets up the response and request objects, and ensures only Admin and Support user types can access this controller.
        /// </summary>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        public DailyAuspiciousTimeUpdateController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if (!new List<int> { 1, 2 }.Contains(request.user_type_id))//Only Admin & support
                throw new ErrorException("Not allowed");
        }

        
        /// <summary>
        /// Inserts a new daily auspicious time update entry.
        /// Validates the transaction date and rashi details, ensures no duplicate entry for the date,
        /// and saves the data to the database. Only accessible by Admin and Support user types.
        /// </summary>
        /// <param name="data">The daily auspicious time update model containing transaction date and rashi details.</param>
        /// <returns>An <see cref="IActionResult"/> containing the operation result and message.</returns>
        [HttpPost]
        [Route("create")]
        public IActionResult Insert([FromBody] DailyAuspiciousTimeUpdateModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel);

                data._id = null;
                DateTime.TryParse(data.transaction_date, out DateTime _transaction_date);

                if (_transaction_date == DateTime.MinValue)
                {
                    throw new ErrorException("Enter valid date i.e yyyy-MM-dd.");
                }

                //Check if date already exists
                var filter = Builders<DailyAuspiciousTimeUpdateModel>.Filter.Eq("transaction_date", data.transaction_date);
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
                _ = MongoLogRecorder.RecordLogAsync<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel, data._id, request.user_id);

                response.message = "Daily Auspicious Time Updates saved for the day.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        /// <summary>
        /// Loads the base data required for daily auspicious time updates, specifically the list of rashis.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the base data for daily auspicious time updates.</returns>
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
        /// Updates an existing daily auspicious time update entry.
        /// Validates the transaction date, rashi details, and entry id, then updates the data in the database.
        /// Only accessible by Admin and Support user types.
        /// </summary>
        /// <param name="data">The daily auspicious time update model containing transaction date, rashi details, and entry id.</param>
        /// <returns>An <see cref="IActionResult"/> containing the operation result and message.</returns>
        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> UpdateAsync([FromBody] DailyAuspiciousTimeUpdateModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel);

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
                var filter = Builders<DailyAuspiciousTimeUpdateModel>.Filter.Eq("_id", id);
                //var result = await col.UpdateOneAsync(filter,data.ToBsonDocument());

                var update = Builders<DailyAuspiciousTimeUpdateModel>.Update
                                .Set("items", data.items)
                                .Set("updated_date",DateTime.Now)
                                .Set("updated_by",request.user_id);

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel, data._id, request.user_id);


                //col.InsertOne(data);
                response.message = "Daily Auspicious Time saved for the day.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        /// <summary>
        /// Retrieves the list of all daily auspicious time update entries.
        /// Each entry includes user-friendly names for the creator and updater.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of daily auspicious time update entries.</returns>
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAllList()
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel);
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
        /// Retrieves a specific daily auspicious time update entry by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the daily auspicious time update entry.</param>
        /// <returns>An <see cref="IActionResult"/> containing the requested daily auspicious time update entry.</returns>
        [HttpGet]
        [Route("Get")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel);

                var obj_id = new ObjectId(id);

                var filter = Builders<DailyAuspiciousTimeUpdateModel>.Filter.Eq("_id", obj_id);
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
