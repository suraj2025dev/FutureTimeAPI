using Auth.MongoRef;
using Library.Data;
using Library.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace Auth
{
    public static class AuthorizationManagement
    {
        public static ApplicationResponse ManageAuth(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext context)
        {
            ApplicationResponse? response = null;
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                var actionAttributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true);
                if (actionAttributes.Any(a => a is AnonymousAuthorizeFilter)) return null;
                if (actionAttributes.Any(a => a is GuestAuthFilter))
                {
                    string token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    context.HttpContext.Items["guest_token"] = token;
                    return null;
                }
                var endpoint = context.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IEndpointFeature>()?.Endpoint; // Updated to use IEndpointFeature
                var hasAllowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

                if (hasAllowAnonymous)
                {
                    return null; // Fixed return statement
                }
            }

            // USER BASED AUTH
            var dependencyScope = context.HttpContext.RequestServices;
            var active_session = new SessionPayload();

            try
            {
                string token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Guid as_unique_identifier = Guid.Empty;
                {
                    active_session = SessionManagement.IsSessionActive(token);
                    if (active_session == null)
                    {
                        throw new Exception("Authentication failure.");
                    }
                }

                context.HttpContext.Items["user_email"] = active_session.user_email;
                context.HttpContext.Items["user_id"] = active_session.user_id;
                context.HttpContext.Items["user_type_id"] = active_session.user_type_id;

                // User from mongo
                var user = GetUser(active_session.user_email);

                // If user is blocked/locked/inactive. His session is terminated here.
                if (user == null)
                {
                    SessionManagement.ClearUpSpecificSessionOfUser(active_session.user_email);
                    throw new Exception("Session Terminated");
                }
            }
            catch (Exception ex)
            {
                ex.GenerateResponse();
                active_session = null;
            }
            finally
            {
                if (active_session == null)
                {
                    response = new ApplicationResponse("401");
                }
            }
            return response;
        }

        private static UsersModel? GetUser(string email)
        {
            var col = MongoDbRef.ConnectCollection<UsersModel>(MongoDbRef.COLLECTION_NAME.UsersModel);
            string pattern = $"^{Regex.Escape(email)}$";
            var filter = Builders<UsersModel>.Filter.And(
                            Builders<UsersModel>.Filter.Regex("email", new BsonRegularExpression(pattern, "i")),
                            Builders<UsersModel>.Filter.Eq("active", true)
                        );
            var item = col.Find(filter).FirstOrDefaultAsync().Result;
            return item;
        }
    }
}
