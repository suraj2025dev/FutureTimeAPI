using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Library.Data;
using Microsoft.AspNetCore.Authorization;

namespace FutureTime.Filters
{

    public class ApplicationAuthorizationFilter : Attribute, IAuthorizationFilter
    {
        //IRoleService rolesService;
        public ApplicationAuthorizationFilter()
        {  
            
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            var hasAllowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

            if(hasAllowAnonymous)
            {
                return;
            }

            ApplicationResponse? response;

            #region USERS

            response = Auth.AuthorizationManagement.ManageAuth(context);
            if (response != null)
            {
                //response = new PulseResponse("401");
                var jsonResponse = new JsonResult(response);
                context.Result = jsonResponse;
            }
            #endregion
        }
    }
}
