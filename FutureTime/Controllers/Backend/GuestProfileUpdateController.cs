﻿
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
using FutureTime.Helper;
using FutureTime.Service;

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class GuestProfileUpdateController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;
        private readonly FirebaseService _firebaseService;

        public GuestProfileUpdateController(
            IHttpContextAccessor httpContextAccessor, 
            FirebaseService firebaseService)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if (!new List<int> { 1, 2 }.Contains(request.user_type_id))//Only Admin & support
                throw new ErrorException("Not allowed");
            _firebaseService = firebaseService;
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
                        lucky_number = (data.lucky_number ?? "").Trim(),
                        rashi_id=data.rashi_id,
                        compatibility_description = data.compatibility_description
                    })
                    .Set("updated_date", DateTime.Now)
                    .Set("updated_by", request.user_id);

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel, data.guest_id, request.user_id);

                var success = await _firebaseService.PushNotificationAsync("Profile Verified", "Your profile has been verified.", null, data.guest_id);

                if(!success)
                {
                    Console.WriteLine("GuestProfileUpdateController, Failed to send notification.");
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

        [HttpPost]
        [Route("PlanetDetailAPIForGuest")]
        public async Task<IActionResult> PlanetDetailAPIForGuest(string guest_id)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);


                //Check if date already exists
                var filter = Builders<GuestsModel>.Filter.Eq("_id", guest_id);

                var guest = col.Find(filter).First();

                var planet_detail = await VedicAPIConnection.APICall.GetPlanetDetail(
                        DateTime.Parse(guest.dob),
                        guest.tob,
                        guest.city.lng,
                        guest.city.lat,
                        guest.tz.ToString(),
                        "en"
                    );

                UpdateDefinition<GuestsModel> update;
                update = Builders<GuestsModel>.Update
                    .Set(u => u.api_planet_detail, planet_detail.ToString());

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }
                _ = MongoLogRecorder.RecordLogAsync<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel, guest_id, request.user_id);


                //col.InsertOne(data);
                response.message = "Planet Detail Fetched.";
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

                var all_users = await UsersHelper.GetAllUserAsync();


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
                    filters.Add(Builders<GuestsModel>.Filter.Regex("name", Helper.Lib._BsonRegularExpression(data.name.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.email))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("email", Helper.Lib._BsonRegularExpression(data.email.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.city_id))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("city_id", Helper.Lib._BsonRegularExpression(data.city_id.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.dob))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("dob", Helper.Lib._BsonRegularExpression(data.dob.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.tob))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("tob", Helper.Lib._BsonRegularExpression(data.tob.ToLower(), "i")));
                }
                if (!string.IsNullOrEmpty(data.name))
                {
                    filters.Add(Builders<GuestsModel>.Filter.Regex("name", Helper.Lib._BsonRegularExpression(data.name.ToLower(), "i")));
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

                items.ForEach(f => {
                    f.updated_by = UsersHelper.GetUserName(all_users, f.updated_by);
                    f.created_by = UsersHelper.GetUserName(all_users, f.created_by);
                });

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
