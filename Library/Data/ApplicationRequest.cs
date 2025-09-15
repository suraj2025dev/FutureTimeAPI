namespace Library.Data
{
    public class ApplicationRequest
    {
        public ApplicationRequest()
        {
            error_code = "0";
            message = "";
            data = new Dictionary<string, object>();
            user_email = null;
        }
        public string error_code { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> data { get; set; }
        public string user_id { get; set; }
        public int user_type_id { get; set; }
        public string user_email { get; set; }
        public string ip_address { get; set; }
        public DateTime ac_date { get; set; }
        public DateTime created_date { get; set; }
        public string guest_token { get; set; }
        public string guest_id { get; set; }

    }
}
