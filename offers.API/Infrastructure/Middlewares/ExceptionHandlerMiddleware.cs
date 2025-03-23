using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions;
using offers.Application.Exceptions.Category;

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

            var result = JsonConvert.SerializeObject(error, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = error.Status!.Value;

            await context.Response.WriteAsync(result);
        }
    }

    public class ApiError : ProblemDetails
    {
        private HttpContext _httpContext;
        private Exception _exception;
        public LogLevel LogLevel { get; set; }

        public string? TraceId
        {
            get
            {
                Extensions.TryGetValue("TraceId", out var traceId);
                return (string?)traceId;
            }

            set => Extensions["TraceId"] = value;
        }

        public List<string>? Errors
        {
            get
            {
                Extensions.TryGetValue("Errors", out var errors);
                return (List<string>?)errors;
            }

            set => Extensions["Errors"] = value;
        }

        public ApiError(HttpContext httpContext, Exception exception)
        {
            _httpContext = httpContext;
            _exception = exception;

            TraceId = httpContext.TraceIdentifier;
            Instance = httpContext.Request.Path;

            Status = (int)HttpStatusCode.InternalServerError;
            Title = "there was an error on the server"; 
            LogLevel = LogLevel.Error;
            HandleException((dynamic)exception);
        }

        private void HandleException(CompanyAlreadyActiveException exception)
        {
            Type = "https://example.com/probs/Conflict";
            Title = exception.Message;
            Status = StatusCodes.Status409Conflict;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(AccountAlreadyExistsException exception)
        {
            Type = "https://example.com/probs/Conflict";
            Title = exception.Message;
            Status = StatusCodes.Status409Conflict;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(AccountCouldNotBeCreatedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(AccountCouldNotBePatchedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(AccountCouldNotValidateException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            Errors = exception.Errors;
            LogLevel = LogLevel.Information;
        }

        private void HandleException(AccountDoesNotExistException exception)
        {
            Type = "https://example.com/probs/Not-Found";
            Title = exception.Message;
            Status = StatusCodes.Status404NotFound;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(AccountNotFoundException exception)
        {
            Type = "https://example.com/probs/Not-Found";
            Title = exception.Message;
            Status = StatusCodes.Status404NotFound;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(CategoryAlreadyExistsException exception)
        {
            Type = "https://example.com/probs/Conflict";
            Title = exception.Message;
            Status = StatusCodes.Status409Conflict;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(CategoryCouldNotBeCreatedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(CategoryCouldNotValidateException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            Errors = exception.Errors;
            LogLevel = LogLevel.Information;
        }
    }
}
