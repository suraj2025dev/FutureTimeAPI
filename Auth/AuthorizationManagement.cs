using Library.Data;
using Library.Extensions;
using Library;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Library.Exceptions;
using static System.Net.WebRequestMethods;
using System.Web.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using Auth.MongoRef;

namespace Auth
{
    public static class AuthorizationManagement
    {
        public static ApplicationResponse ManageAuth(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext context)
        {
            ApplicationResponse ?response  = null;
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
            }




            //USER BASED AUTH
            var dependencyScope = context.HttpContext.RequestServices;
            //var userRepo = dependencyScope.GetService(typeof(IUserRepo)) as User.;

            var active_session = new SessionPayload();

            try
            {
                string token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Guid as_unique_identifier = Guid.Empty;//Empty by default

                //var handler = new JwtSecurityTokenHandler();

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

                //User from mongo
                var user = GetUser(active_session.user_email);
                

                //If user is blocked/locked/inactivate. His session is terminated here.
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

            var filter = Builders<UsersModel>.Filter.And(
                            Builders<UsersModel>.Filter.Regex("email", new BsonRegularExpression(email, "i")),
                            Builders<UsersModel>.Filter.Eq("active", true)
                        );
            var item = col.Find(filter).FirstOrDefaultAsync().Result;
            return item;
        }
    }
}
