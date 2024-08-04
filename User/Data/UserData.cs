using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Data
{
    public class UserData
    {
        public DateTime forget_password_otp_valid_till { get; set; }

        public Guid uid { get; set; }
        public Int32 id { get; set; }

        [Required(ErrorMessage = "Please enter full name")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Full name must be 1 to 100 character long.")]
        public string full_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string confirm_password { get; set; }
        public string forget_password_otp { get; set; }
        public bool is_verified { get; set; }
        public bool is_locked { get; set; }
        public bool is_blocked { get; set; }
        public DateTime ac_date { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }
        public string created_by { get; set; }
        public DateTime created_date { get; set; }
    }
}
