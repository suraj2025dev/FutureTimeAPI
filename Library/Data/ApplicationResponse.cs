using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data
{
    public class ApplicationResponse
    {
        /// <summary>
        /// 0: Success, 1: Error, 2: Warning, 3: Info
        /// </summary>
        public string error_code { get; set; }
        /// <summary>
        /// 200: Request Completed, 404: Resource not avaliable, 401: Unauthenticated, 403: Unauthorized, 500: Unexpected Error.
        /// </summary>
        public string status_code { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string description { get; set; }
        public Dictionary<string, object> data { get; set; }

        public ApplicationResponse()
        {
            data = new Dictionary<string, object>();
            error_code = "0";
            status_code = "200";
            message = "";
        }

        public ApplicationResponse(string status_code, string error_code, string message = "", Dictionary<string, object> data = null)
        {
            this.status_code = status_code;
            this.error_code = error_code;
            this.message = message;
            this.data = data;

        }

        public ApplicationResponse(string status_code, Dictionary<string, object> data = null)
        {
            this.status_code = status_code;
            this.data = data;
            switch (status_code)
            {
                case "200":
                    error_code = "0";
                    message = "Success.";
                    break;
                case "404":
                    error_code = "1";
                    message = "Resource not avaliable.";
                    break;
                case "401":
                    error_code = "1";
                    message = "You are not authenticated.";
                    break;
                case "403":
                    error_code = "1";
                    message = "You are not authorized.";
                    break;
                case "400":
                    error_code = "1";
                    message = "Bad Request.";
                    break;
                case "500":
                    error_code = "1";
                    message = "Unexpected Error.";
                    break;
            }
        }

    }
}
