namespace FutureTime.MongoDB.Model
{
    public class GuestLoginData
    {
       
        public string name { get; set; }
        public string email { get; set; }
        public string city_id { get; set; }
        public decimal tz { get; set; }
        public string dob { get; set; }
        public string tob { get; set; }
        public string gender { get; set; }
        public bool is_login { get; set; }
    }

}
