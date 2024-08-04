using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace User.Data
{
    public class UserSignUpDTO
    {

        [Required(ErrorMessage = "Please enter full name")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Full name must be 1 to 100 character long.")]
        public string full_name { get; set; }

        [Required(ErrorMessage = "Please enter email.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email.")]
         public string email { get; set; }


        [Required(ErrorMessage = "Please enter password")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "password must be 1 to 100 character long.")]
        public string password { get; set; }


        [Required(ErrorMessage = "Please enter password")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "password must be 1 to 100 character long.")]
        public string confirm_password { get; set; }
    }
}
