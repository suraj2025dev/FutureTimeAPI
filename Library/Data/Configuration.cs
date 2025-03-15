namespace Library.Data
{
    public class Configuration
    {
        public APP App { get; set; }
    }
    public class APP
    {
        public Database Database { get; set; }
        public MongoDB MongoDB { get; set; }
        public Email Email { get; set; }
        public int SessionTimeOut { get; set; }
        public VedicAPI VedicAPI { get; set; }
    }
    public class MongoDB
    {
        public string ConnectionURL { get; set; }
        public string DatabaseName { get; set; }
    }
    public class Database
    {
        public string HOST { get; set; }
        public string PORT { get; set; }
        public string NAME { get; set; }
        public string PASSWORD { get; set; }
    }

    public class Email
    {
        public string HOST { get; set; } 
        public string PORT { get; set; }
        public string PASSWORD { get; set; }
        public string MAIL { get; set; }
        public int SecureSocketOptions { get; set; }
        public string SenderName { get; set; }
    }

    public class VedicAPI
    {
        public string URL { get; set; }
        public string apiKey { get; set; }
    }

}
