using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using Library.Data;
using MongoDB.Driver;

namespace FutureTime
{
    public static class BaseLib
    {
        public static ApplicationRequest FillSessionDetail(this IHttpContextAccessor contextAccessor, ApplicationRequest request)
        {
            request.user_email = GetUserDetail(contextAccessor,"user_email");
            request.user_id = GetUserDetail(contextAccessor,"user_id");
            int.TryParse(GetUserDetail(contextAccessor, "user_type_id"),out int user_type_id);
            request.user_type_id = user_type_id;
            request.ip_address = GetIPAddress(contextAccessor);
            return request;
        }

        public static ApplicationRequest FillGuestSessionAsync(this IHttpContextAccessor contextAccessor, ApplicationRequest request)
        {
            request.guest_token = GetUserDetail(contextAccessor, "guest_token");
            //Get ID From Token
            var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

            try
            {
                var filter1 = Builders<GuestsModel>.Filter.Eq("token", Guid.Parse(request.guest_token));
                var item = col.FindAsync(filter1).Result.FirstOrDefault();

                request.guest_id = item._id;
                request.ip_address = GetIPAddress(contextAccessor);
            }
            catch
            {

            }
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
