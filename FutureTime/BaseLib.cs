using Library.Data;
using Microsoft.AspNetCore.Http;

namespace FutureTime
{
    public static class BaseLib
    {
        public static ApplicationRequest FillSessionDetail(this IHttpContextAccessor contextAccessor, ApplicationRequest request)
        {
            request.user_email = GetUserDetail(contextAccessor,"user_email");
            request.user_id = GetUserDetail(contextAccessor,"user_id");
            request.ip_address = GetIPAddress(contextAccessor);
            return request;
        }

        public static string GetUserDetail(IHttpContextAccessor iHttpContextAccessor, string key)
        {
            try
            {
                if (iHttpContextAccessor.HttpContext.Items.ContainsKey(key))
                    return iHttpContextAccessor.HttpContext.Items[key].ToString();
                else
                    return null;
            }
            catch (Exception e)
            {//For those which action method uses anonymous login.
                return null;
            }
        }

        public static string GetIPAddress(IHttpContextAccessor iHttpContextAccessor)
        {
            var ip = iHttpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString();

            var forwared_ips = ip.Split(",");

            ip = forwared_ips[0];

            if (ip == "")
                ip = iHttpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            return ip;
        }
    }

}
