using Library.Data;
using Library.Exceptions;
using System.Text.Json;

namespace FutureTime
{
    

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Process request
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex); // Handle any unhandled exception
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception (optional)
            Console.WriteLine($"Error: {exception.Message}");
            ApplicationResponse response = new ApplicationResponse();
            if (exception.GetType() == typeof(ErrorException))
            {
                var ex_parsed = (ErrorException)exception;
                response.error_code = ((int)ex_parsed.exception_result).ToString();
                response.data = ex_parsed.data;
                response.message = exception.Message;
                response.status_code = "200";
            }

            // Serialize the response to JSON
            var result = JsonSerializer.Serialize(response);

            // Set the response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;// (int)HttpStatusCode.InternalServerError;

            // Send the JSON response
            return context.Response.WriteAsync(result);
        }
    }

}
