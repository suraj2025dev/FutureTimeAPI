using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Library.Data
{
    public class MailDTO
    {
        public List<string> EmailTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string DisplayName { get; set; }

    }

    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }
        public int SecureSocketOptions { get; set; }
    }
}
