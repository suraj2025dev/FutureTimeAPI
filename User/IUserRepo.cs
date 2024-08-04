using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Library.Data;
using System.Threading.Tasks;

namespace User
{
    public interface IUserRepo
    {
        ApplicationResponse Login(ApplicationRequest request);
        ApplicationResponse Signup(ApplicationRequest request);
        ApplicationResponse VerifyOTPResetPassword(ApplicationRequest request);
        ApplicationResponse ForgetPassword(ApplicationRequest request);

    }
}
