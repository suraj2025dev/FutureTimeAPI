using FutureTime.Filters;
using Auth;
using Library;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using User;
using User.Data;
using static System.Net.WebRequestMethods;
using FutureTime.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using FutureTime.MongoDB.Model;

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

            

            var filter = Builders<UsersModel>.Filter.And(
                                Builders<UsersModel>.Filter.Regex("email", new BsonRegularExpression(data.email, "i")),
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


        /// <summary>
        /// use for the new user signup
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("Signup")]
        [AnonymousAuthorizeFilter]
        [ValidateModel]
        //need to validate the model for the validation --for now skip
        public IActionResult Signup([FromBody] UserSignUpDTO data)
        {
            request.data.Add("data", data);
            response = this.service.Signup(request);
            return Ok(response);
        }


        /// <summary>
        /// use for the verification of the OTP
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("VerifyOTPResetPassword")]
        [AnonymousAuthorizeFilter]
        public IActionResult VerifyOTPResetPassword([FromBody] VerifyOTPResetPasswordDTO data)
        {
            request.data.Add("data", data);
            response = this.service.VerifyOTPResetPassword(request);
            return Ok(response);
            
        }



        /// <summary>
        /// use for the forget password scenario with otp generation 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        [HttpPost("ForgetPassword")]
        [AnonymousAuthorizeFilter]
        public IActionResult ForgetPassword([FromBody] ForgetPasswordDTO data)
        {
            request.data.Add("data", data);
            response = this.service.ForgetPassword(request);
            return Ok(response);
        }

        [HttpGet]
        public IActionResult SessionCheck()
        {
            return Ok(response);
        }
    }
}
