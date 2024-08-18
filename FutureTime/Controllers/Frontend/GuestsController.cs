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
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace FutureTime.Controllers.Backend
{
    [Route("frontend/[controller]")]
    public class GuestsController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public GuestsController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillGuestSessionAsync(request);

        }

        [AnonymousAuthorizeFilter]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginAsync([FromBody] GuestLoginData data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

                var otp = GenerateOTP();

                if (data.is_login)
                {
                    //Check if email already exists.
                    var filter = Builders<GuestsModel>.Filter.Regex("email", new BsonRegularExpression(data.email.ToLower(), "i"));
                    var guestData = col.Find(filter).ToList();

                    if (guestData != null && guestData.Count() > 0)
                    {
                        //Start Login Process, Generate Token & forward to APP.

                        var guestDataFilter = Builders<GuestsModel>.Filter.Eq("_id", guestData[0]._id);

                        UpdateDefinition<GuestsModel> update;

                        update = Builders<GuestsModel>.Update
                            .Set(u => u.otp, otp);

                        var result = await col.UpdateOneAsync(filter, update);

                        if (result.MatchedCount > 0)
                        {
                            response.message = "OTP is sent to your email.";
                        }
                    }
                    else
                    {
                        throw new ErrorException("Email not registered.");
                    }
                }
                else
                {
                    var filter = Builders<GuestsModel>.Filter.Regex("email", new BsonRegularExpression(data.email.ToLower(), "i"));
                    var guestData = col.Find(filter).ToList();

                    if (guestData != null && guestData.Count() > 0)
                    {
                        throw new ErrorException("Email already registered. Please login.");
                    }
                    //Start to create new guest

                    if (data.name == null || data.name == "")
                    {
                        throw new ErrorException("Please provide name");
                    }

                    if (data.email == null || data.email == "")
                    {
                        throw new ErrorException("Please provide email");
                    }

                    if (data.city_id == null || data.city_id == "")
                    {
                        throw new ErrorException("Please provide city");
                    }


                    if (data.tob == null || data.tob == "")
                    {
                        throw new ErrorException("Please provide tob");
                    }

                    if (data.dob == null || data.dob == "")
                    {
                        throw new ErrorException("Please provide dob");
                    }

                    if(!DateTime.TryParse(data.dob, out DateTime _dob))
                    {
                        throw new ErrorException("Please provide valid dob in format like 2024-01-01");
                    }

                    string time_pattern = @"^([01]\d|2[0-3]):([0-5]\d)$";
                    Regex regex = new Regex(time_pattern);
                    if (!regex.IsMatch(data.tob))
                    {
                        throw new ErrorException("Please provide valid tob in HH:MM format from 00:00 to 23:59");
                    }

                    //data.token = (Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString()).Replace("-","");

                    col.InsertOne(new GuestsModel
                    {
                        active= true,
                        city_id= data.city_id,
                        dob= data.dob,
                        email= data.email,
                        name = data.name,
                        otp=otp,
                        tob = data.tob,
                        token=null
                    });

                    response.message = "New profile created. OTP is sent to your email.";
                }

                // Call SendMail in the background
                _ = Task.Run(() =>
                {
                    try
                    {
                        Library.EmailManagement.SendMail(new MailDTO
                        {
                            Body = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {
            font-family: 'Roboto', sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f5f5f5;
        }
        .container {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            padding: 20px;
        }
        .card {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            max-width: 400px;
            width: 100%;
            padding: 20px;
            text-align: center;
        }
        .card h1 {
            color: #3f51b5;
            font-size: 24px;
            margin-bottom: 20px;
        }
        .card p {
            color: #333333;
            font-size: 16px;
            margin: 10px 0;
        }
        .otp {
            background-color: #e8eaf6;
            border-radius: 4px;
            color: #3f51b5;
            display: inline-block;
            font-size: 24px;
            font-weight: bold;
            padding: 10px 20px;
        }
        .thank-you {
            color: #9e9e9e;
            font-size: 14px;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""card"">
            <h1>Future Time</h1>
            <p>Please use this OTP to login: <span class=""otp"">"+otp+@"</span></p>
            <p class=""thank-you"">Thank you for using our service!</p>
        </div>
    </div>
</body>
</html>

",
                            DisplayName = data.name,
                            EmailTo = new List<string> { data.email },
                            Subject = "FutureTime Login | OTP"
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log or handle exceptions as needed
                        Console.WriteLine("Error sending email: " + ex.Message);
                    }
                });


            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }



            return Ok(response);

        }

        private static string GenerateOTP()
        {
            Random random = new Random();
            int otp = random.Next(100000, 1000000); // Generates a number between 100000 and 999999
            return otp.ToString();
        }

        [AnonymousAuthorizeFilter]
        [HttpGet]
        [Route("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(string email, string otp)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

                var filter1 = Builders<GuestsModel>.Filter.Eq("email", email);
                var filter2 = Builders<GuestsModel>.Filter.Eq("otp", otp);
                var item = await col.Find(filter1 & filter2).FirstOrDefaultAsync();

                if (item == null) {
                    throw new ErrorException("Email / OTP is invalid.");
                }

                var token = Guid.NewGuid();

                var guestDataFilter = Builders<GuestsModel>.Filter.Eq("_id", item._id);

                UpdateDefinition<GuestsModel> update;

                update = Builders<GuestsModel>.Update
                    .Set(u => u.otp, null)//Reset OTP
                    .Set(u => u.token, token);

                var result = await col.UpdateOneAsync(guestDataFilter, update);

                if (result.MatchedCount > 0)
                {
                    response.message = "Logged In.";
                }

                response.data.Add("token", token.ToString());
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [AnonymousAuthorizeFilter]
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

        

        [GuestAuthFilter]
        [HttpGet]
        [Route("Get")]
        public async Task<IActionResult> Get()
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }
            try
            {
                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

                var obj_id = new ObjectId(request.guest_id);

                var filter = Builders<GuestsModel>.Filter.Eq("_id", obj_id);
                var item = await col.Find(filter).FirstOrDefaultAsync();
                var all_rashi = FTStaticData.data.Where(w => w.type == STATIC_DATA_TYPE.RASHI).Select(s => s.list).First();
                response.data.Add("item", new { 
                    name=item.name,
                    email=item.email,
                    dob=item.dob,
                    tob=item.tob,
                    city_id=item.city_id,
                    city="",
                    guest_profile = item.guest_profile == null ? null 
                        : new {
                            basic_description=item.guest_profile.basic_description,
                            lucky_color=item.guest_profile.lucky_color,
                            lucky_gem = item.guest_profile.lucky_gem,
                            lucky_number = item.guest_profile.lucky_number,
                            rashi_id = item.guest_profile.rashi_id,
                            rashi_name = all_rashi.Where(w=>w.id == item.guest_profile.rashi_id.ToString()).Select(s=>s.name).FirstOrDefault(),
                            item.guest_profile.compatibility_description
                        }
                });
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [GuestAuthFilter]
        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] GuestLoginData data)
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }
            try
            {
                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

                var obj_id = new ObjectId(request.guest_id);

                var filter = Builders<GuestsModel>.Filter.Eq("_id", obj_id);
                var item = await col.Find(filter).FirstOrDefaultAsync();

                var update = Builders<GuestsModel>.Update
                    .Set(u => u.name, data.name)
                    .Set(u => u.city_id, data.city_id)
                    .Set(u => u.tob, data.tob)
                    .Set(u => u.dob, data.dob);

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount > 0)
                {
                    response.message = "Profile updated";
                }
                else
                {
                    throw new ErrorException("Profile not found.");
                }
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [GuestAuthFilter]
        [HttpGet]
        [Route("GetDashboardData")]
        public async Task<IActionResult> GetDashboardData(string date)
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }
            try
            {

                var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

                var obj_id = new ObjectId(request.guest_id);

                var filter = Builders<GuestsModel>.Filter.Eq("_id", obj_id);
                var guest = await col.Find(filter).FirstOrDefaultAsync();
                if (guest.guest_profile == null)
                {

                }
                else
                {
                    var rashi_id = guest.guest_profile.rashi_id;

                    var col_aus = MongoDBService.ConnectCollection<DailyAuspiciousTimeUpdateModel>(MongoDBService.COLLECTION_NAME.DailyAuspiciousTimeUpdateModel);

                    var filter_aus = Builders<DailyAuspiciousTimeUpdateModel>.Filter.Eq("transaction_date", date);
                    var item_aus = await col_aus.Find(filter_aus).FirstOrDefaultAsync();

                    //var col_com = MongoDBService.ConnectCollection<DailyCompatibilityUpdateModel>(MongoDBService.COLLECTION_NAME.DailyCompatibilityUpdateModel);

                    //var filter_com = Builders<DailyCompatibilityUpdateModel>.Filter.Eq("transaction_date", date);
                    //var item_com = await col_com.Find(filter_com).FirstOrDefaultAsync();

                    var col_kun = MongoDBService.ConnectCollection<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel);

                    var filter_kun = Builders<DailyHoroscopeUpdatesModel>.Filter.Eq("transaction_date", date);
                    var item_kun = await col_kun.Find(filter_kun).FirstOrDefaultAsync();

                    response.data.Add("horoscope", item_kun==null?null:item_kun.items.Where(w => w.rashi_id == rashi_id).FirstOrDefault());
                    response.data.Add("compatibility", guest.guest_profile.compatibility_description);
                    response.data.Add("auspicious", item_aus == null ? null : item_aus.items.Where(w => w.rashi_id == rashi_id).FirstOrDefault());
                }

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
