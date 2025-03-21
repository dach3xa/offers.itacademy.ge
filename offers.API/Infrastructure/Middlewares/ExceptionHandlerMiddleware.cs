using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using Newtonsoft.Json;

namespace offers.API.Infrastructure.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var error = new ApiError(context, ex);

            var result = JsonConvert.SerializeObject(error);

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = error.Status!.Value;

            await context.Response.WriteAsync(result);
        }
    }

    public class ApiError : ProblemDetails
    {
        public const string UnhandlerErrorCode = "UnhandledError";
        private HttpContext _httpContext;
        private Exception _exception;

        public LogLevel LogLevel { get; set; }
        public string Code { get; set; }

        public string? TraceId
        {
            get
            {
                if (Extensions.TryGetValue("TraceId", out var traceId))
                {
                    return (string?)traceId;
                }

                return null;
            }

            set => Extensions["TraceId"] = value;
        }

        public ApiError(HttpContext httpContext, Exception exception)
        {
            _httpContext = httpContext;
            _exception = exception;

            TraceId = httpContext.TraceIdentifier;

            //default
            Code = UnhandlerErrorCode;
            Status = (int)HttpStatusCode.InternalServerError;
            Title = "მოხდა შეცდომა სერვერზე"; //TODO ეს მესიჯი არ უნდა ნახოს.
            LogLevel = LogLevel.Error;
            Instance = httpContext.Request.Path;
            HandleException((dynamic)exception); //TODO
        }

        private void HandleException(PersonNotFoundException exception)
        {
            Code = exception.Code;
            Status = (int)HttpStatusCode.NotFound;
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4";
            Title = exception.Message;
            LogLevel = LogLevel.Information;
        }

        private void HandleException(PersonAlreadyExistsException exception)
        {
            Code = exception.Code;
            Status = (int)HttpStatusCode.Conflict;
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.9";
            Title = exception.Message;
            LogLevel = LogLevel.Information;
        }

        //private void HandleException(InvalidCompanyNameException exception)
        //{
        //    Code = exception.Code;
        //    Status = (int)HttpStatusCode.BadRequest;
        //    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        //    Title = exception.Message;
        //    LogLevel = LogLevel.Information;
        //}

        private void HandleException(Exception exception)
        {

        }
    }
}
