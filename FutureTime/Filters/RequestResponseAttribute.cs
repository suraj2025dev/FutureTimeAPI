using System;
using System.Collections.Generic;
using System.Linq;
using Auth;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Org.BouncyCastle.Asn1.Ocsp;

namespace AegisService.Lib.Filters
{
    public class RequestResponseFilterAttribute : TypeFilterAttribute
    {
        public RequestResponseFilterAttribute() :
        base(typeof(ResponseActionFilterImpl))
        {
        }

        private class ResponseActionFilterImpl : IActionFilter
        {
            public void OnActionExecuting(ActionExecutingContext context)
            {
                ApplicationRequest request = null;


                if (context.ActionArguments.TryGetValue("request", out object requestParams))
                {
                    request = (ApplicationRequest)requestParams;
                    request.user_email = context.HttpContext.Items["user_email"].ToString();
                }
                context.ActionArguments["request"] = request;
                
                //Validate Model if ValidateModel is present
                var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                if (controllerActionDescriptor != null)
                {
                    var actionAttributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true);
                    if (actionAttributes.Any(a => a is ValidateModelAttribute))
                    {
                        if (context.ModelState.IsValid == false)
                        {
                            var modelValidations = new List<ModelValidation>();

                            var errors = context.ModelState.Select(x => new {
                                Key = x.Key,
                                Errors = x.Value.Errors
                            })
                          .ToList();

                            foreach (var error in errors)
                            {

                                var modelValidation = new ModelValidation();

                                modelValidation.key = error.Key;// ValidationErrors;
                                modelValidation.error_messages = new List<string>();
                                foreach (var error_message in error.Errors)
                                {
                                    modelValidation.error_messages.Add(error_message.ErrorMessage);
                                }

                                modelValidations.Add(modelValidation);

                            }
                            var data = new Dictionary<string, object>();
                            data.Add("model_validation", modelValidations);
                            var response = new ApplicationResponse("400", data);
                            response.message = modelValidations.Count()>0? string.Join(',',modelValidations[0].error_messages):"";

                            var jsonResponse = new JsonResult(response);
                            context.Result = jsonResponse;


                        }
                    }

                }


            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.Result != null)
                {
                    if (context.Result.GetType() == typeof(FileContentResult) || context.Result.GetType() == typeof(FileStreamResult))
                    {

                    }
                    else
                    {
                        // perform some business logic work
                        var myResult = (OkObjectResult)context.Result;

                        //Add type checking here... sample code only
                        //Modiy object values
                        try
                        {
                            ApplicationResponse response = (ApplicationResponse)myResult.Value;
                            var continue_request = true;

                            var token = context.HttpContext.Items["renewed_jwt_token"];

                            if (token != null)
                            {
                                response.data.Add("session_status", new
                                {
                                    token = token,
                                    continue_request = continue_request
                                });
                            }
                            else
                            {
                                response.data.Add("session_status", new
                                {
                                    continue_request = continue_request
                                });
                            }



                        }
                        catch
                        {

                        }
                    }
                }//end if of null checking


            }
        }
    }

    public class ModelValidation
    {
        public string key { get; set; }
        public List<string> error_messages { get; set; }
    }
}
