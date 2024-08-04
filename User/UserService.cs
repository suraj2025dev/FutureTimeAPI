using Auth;
using Library;
using Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Data;

namespace User
{
    public class UserService : IUserService
    {

        IUserRepo _repo;
        public UserService(IUserRepo _repo)
        {
            this._repo = _repo;
        }
        ApplicationResponse response;
       


        public ApplicationResponse Login(ApplicationRequest request)
        {
            var data = (UserLoginDTO)request.data["data"];
            var data1 = new UserData
            {
                email=data.email.Trim().ToLower(),
                password=AppLib.CreateSHA256(data.password.Trim()),
                created_by = request.user_id,
                is_deleted = false,  
            };
            request.data["data"] = data1;
            response = _repo.Login(request);
            if (response.error_code == "0")
            {
                var token = SessionManagement.GenerateToken(new SessionPayload
                {
                    user_email = data1.email,
                    last_interaction_time = DateTime.Now,
                    session_id = Guid.NewGuid(),
                    user_id = Dao.ExecuteScalar<int>("select id from pms.tbl_user where lower(email)=@email", new { data1.email })
                });
                response.data.Add("token", token);
            }

            return response;
        }



        public ApplicationResponse Signup(ApplicationRequest request)
        {
            var data = (UserSignUpDTO)request.data["data"];
            var data1 = new UserData
            {
                full_name = data.full_name.Trim().ToUpper(),
                email = data.email.Trim(),
                password = AppLib.CreateSHA256(data.password.Trim()),
                confirm_password = AppLib.CreateSHA256(data.confirm_password.Trim()),
                created_by = request.user_id,
                is_deleted = false,
            };
            request.data["data"] = data1;
            return _repo.Signup(request);
        }



        public ApplicationResponse VerifyOTPResetPassword(ApplicationRequest request)
        {
            //AppStatic.CONFIG.App.Email.HOST;
            var data = (VerifyOTPResetPasswordDTO)request.data["data"];
            var data1 = new UserData
            {   
                email = data.email.Trim(),
                forget_password_otp=data.otp,
                password=AppLib.CreateSHA256(data.new_password)
            };
            request.data["data"] = data1;
            return _repo.VerifyOTPResetPassword(request);
        }



        public ApplicationResponse ForgetPassword(ApplicationRequest request)
        {
            var data = (ForgetPasswordDTO)request.data["data"];
            var data1 = new UserData
            {
                email = data.email.Trim(),
            };
            request.data["data"] = data1;
            return _repo.ForgetPassword(request);
        }

    }
}
