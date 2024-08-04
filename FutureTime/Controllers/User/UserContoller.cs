using FutureTime.Filters;
using Auth;
using Library;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using User;
using User.Data;
using static System.Net.WebRequestMethods;

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
            request.data.Add("data", data);
            response = this.service.Login(request);
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
