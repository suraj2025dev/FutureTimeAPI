using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Library;
using Library.Data;
using User.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Library.Extensions;
using Library.Exceptions;

namespace User
{
    public class UserRepo : IUserRepo
    {
        public ApplicationResponse ForgetPassword(ApplicationRequest request)
        {
            throw new NotImplementedException();
        }

        public ApplicationResponse Login(ApplicationRequest request)
        {
            throw new NotImplementedException();
        }

        public ApplicationResponse Signup(ApplicationRequest request)
        {
            throw new NotImplementedException();
        }

        public ApplicationResponse VerifyOTPResetPassword(ApplicationRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
