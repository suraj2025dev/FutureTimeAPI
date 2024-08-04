using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Data
{
    public class VerifyOTPResetPasswordDTO
    {
        [Required(ErrorMessage = "Please enter email.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email.")]
        public string email { get; set; }


        [Required(ErrorMessage = "Please enter otp")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be of 6 character.")]
        public string otp { get; set; }

        public string new_password { get; set; }

    }
}
