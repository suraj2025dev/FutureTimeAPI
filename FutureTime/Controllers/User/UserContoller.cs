using Auth;
using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using Library.Data;
using Library.Exceptions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using User;
using User.Data;

namespace FutureTime.Controllers.User
{
    [Route("[controller]")]
    public class UserContoller : ControllerBase
    {
        IUserService service;
        ApplicationResponse response;
        ApplicationRequest request;
        public UserContoller(IHttpContextAccessor httpContextAccessor, IUserService service)
        {
            this.service = service;
            response=new ApplicationResponse();
            request= new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
        }


        /// <summary>
        /// Used to login
        /// </summary>
        /// <returns></returns>
        [HttpPost("Login")]
        [AnonymousAuthorizeFilter]
        
        public IActionResult Login([FromBody] UserLoginDTO data)
        {
            var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);


            string email = data.email;
            var filter = Builders<UsersModel>.Filter.And(
                                Builders<UsersModel>.Filter.Regex("email", Helper.Lib._BsonRegularExpression(data.email, "i")),
                                Builders<UsersModel>.Filter.Eq("password", data.password),
                                Builders<UsersModel>.Filter.Eq("active", true)
                            );
            var item = col.Find(filter).FirstOrDefaultAsync().Result;
            
            if(item == null)
            {
                return Ok(new ApplicationResponse { 
                    error_code = "1",
                    message = "Invalid credentials."
                });
            }
            else
            {
                var token = SessionManagement.GenerateToken(new SessionPayload
                {
                    user_email = data.email,
                    last_interaction_time = DateTime.Now,
                    session_id = Guid.NewGuid(),
                    user_id = item._id,
                    user_type_id = item.user_type_id
                });
                response.data.Add("token", token);
                response.data.Add("user_type_id", item.user_type_id);
            }
            return Ok(response);
        }


        ///// <summary>
        ///// use for the new user signup
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //[HttpPost("Signup")]
        //[AnonymousAuthorizeFilter]
        //[ValidateModel]
        ////need to validate the model for the validation --for now skip
        //public IActionResult Signup([FromBody] UserSignUpDTO data)
        //{
        //    request.data.Add("data", data);
        //    response = this.service.Signup(request);
        //    return Ok(response);
        //}


        /// <summary>
        /// use for the verification of the OTP
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("VerifyOTPResetPassword")]
        [AnonymousAuthorizeFilter]
        public async Task<IActionResult> VerifyOTPResetPasswordAsync([FromBody] VerifyOTPResetPasswordDTO data)
        {
            var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);



            var filter = Builders<UsersModel>.Filter.And(
                                Builders<UsersModel>.Filter.Regex("email", Helper.Lib._BsonRegularExpression(data.email, "i")),
                                Builders<UsersModel>.Filter.Regex("forget_password_otp", Helper.Lib._BsonRegularExpression(data.otp, "i")),
                                Builders<UsersModel>.Filter.Eq("active", true)
                            );
            var item = col.Find(filter).FirstOrDefaultAsync().Result;

            if (item == null || item.forget_password_otp_valid_till > DateTime.Now)
            {
                return Ok(new ApplicationResponse
                {
                    error_code = "1",
                    message = "Invalid OTP."
                });
            }
            else
            {
                var update = Builders<UsersModel>.Update
                    .Set(u => u.password, data.new_password)
                    .Set("updated_date", DateTime.Now)
                    .Set("updated_by", request.user_id);
                

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }

                _ = MongoLogRecorder.RecordLogAsync<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel, item._id, request.user_id);

                //col.InsertOne(data);
                response.message = "Password reset successfull.";
            }
            return Ok(response);
        }



        /// <summary>
        /// use for the forget password scenario with otp generation 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        [HttpPost("ForgetPassword")]
        [AnonymousAuthorizeFilter]
        public async Task<IActionResult> ForgetPasswordAsync([FromBody] ForgetPasswordDTO data)
        {
            var col = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);



            var filter = Builders<UsersModel>.Filter.And(
                                Builders<UsersModel>.Filter.Regex("email", Helper.Lib._BsonRegularExpression(data.email, "i")),
                                Builders<UsersModel>.Filter.Eq("active", true)
                            );
            var item = col.Find(filter).FirstOrDefaultAsync().Result;

            if (item == null)
            {
                return Ok(new ApplicationResponse
                {
                    error_code = "1",
                    message = "Invalid Email."
                });
            }
            else
            {
                var otp = GenerateOTP();

                UpdateDefinition<UsersModel> update;

                update = Builders<UsersModel>.Update
                    .Set(u => u.forget_password_otp, otp)
                    .Set(u => u.forget_password_otp_valid_till, DateTime.Now.AddMinutes(30));                

                var result = await col.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new ErrorException("Please provide valid id for update operation.");
                }

                _ = MongoLogRecorder.RecordLogAsync<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel, item._id, request.user_id);
                
                response.message = "OTP is sent to your email";

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
            <h1>ASMORA</h1>
            <p>Please use this OTP to change your password: <span class=""otp"">" + otp + @"</span></p>
            <p class=""thank-you"">This OTP is valid for 30 minutes.</p>
            <p class=""thank-you"">Thank you for using our service!</p>
        </div>
    </div>
</body>
</html>

",
                            DisplayName = "ASMORA",
                            EmailTo = new List<string> { data.email },
                            Subject = "ASMORA Forget Password | OTP"
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log or handle exceptions as needed
                        Console.WriteLine("Error sending email: " + ex.Message);
                    }
                });

            }
            return Ok(response);
        }

        private static string GenerateOTP()
        {
            Random random = new Random();
            int otp = random.Next(100000, 1000000); // Generates a number between 100000 and 999999
            return otp.ToString();
        }

        [HttpGet]
        public IActionResult SessionCheck()
        {
            return Ok(response);
        }
    }
}
