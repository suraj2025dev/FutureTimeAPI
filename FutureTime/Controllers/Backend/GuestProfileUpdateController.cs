
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
using FutureTime.MongoDB.Data;

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class GuestProfileUpdateController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public GuestProfileUpdateController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);

        }

        
        [HttpPost]
        [Route("UpdateGuestProfile")]
        public async Task<IActionResult> UpdateGuestProfile([FromBody] GuestsProfileDTO data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

               
                //Check if date already exists
                var filter = Builders<GuestsModel>.Filter.Eq("_id", data.guest_id);

                UpdateDefinition<GuestsModel> update;
                update = Builders<GuestsModel>.Update
                    .Set(u => u.guest_profile, new GuestsProfileModel { 
                        basic_description=(data.basic_description ?? "").Trim(),
                        lucky_color=(data.lucky_color??"").Trim(),
                        lucky_gem=(data.lucky_gem ?? "").Trim(),
                        lucky_number = (data.lucky_number ?? "").Trim()
                    });

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }



                //col.InsertOne(data);
                response.message = "Guest profile updated.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        [HttpGet]
        [Route("GetAllGuestProfile")]
        public async Task<IActionResult> GetAllGuestProfile(GetAllGuestProfileDTO data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

                int skip = (data.page_number - 1) * data.page_size;

                /*
                public string name { get; set; }
                public string email { get; set; }
                public string city_id { get; set; }
                public string dob { get; set; }
                public string tob { get; set; }
                public bool is_profile_verified { get; set; }
                 */

                //Filter
                var filters = new List<FilterDefinition<GuestsModel>>();
                if (!string.IsNullOrEmpty(data.name))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("name", new BsonRegularExpression(data.name.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.email))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("email", new BsonRegularExpression(data.email.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.city_id))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("city_id", new BsonRegularExpression(data.city_id.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.dob))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("dob", new BsonRegularExpression(data.dob.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.tob))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("tob", new BsonRegularExpression(data.tob.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.name))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("name", new BsonRegularExpression(data.name.ToLower(), "i")));
                }
                if (data.is_profile_verified == true)
                {
                    filters.Add(Builders<GuestsModel>.Filter.Ne(doc => doc.guest_profile, null));
                }
                if (data.is_profile_verified == false)
                {
                    filters.Add(Builders<GuestsModel>.Filter.Eq(doc => doc.guest_profile, null));
                }


                var combinedFilter = filters.Count > 0 ? Builders<GuestsModel>.Filter.And(filters) : Builders<GuestsModel>.Filter.Empty;


                var items = await col.Find(combinedFilter).Skip(skip)
                                        .Limit(data.page_size).ToListAsync();

                var totalCount = await col.CountDocumentsAsync(combinedFilter);

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

    }
}
