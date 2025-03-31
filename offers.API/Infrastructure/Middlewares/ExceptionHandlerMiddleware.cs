using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Account.User;
using offers.Application.Exceptions.Deposit;
using offers.Application.Exceptions.Funds;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Refund;
using offers.Application.Exceptions.Token;
using offers.Application.Exceptions.Transaction;
using System;

namespace offers.API.Infrastructure.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
            _logger.Log(error.LogLevel, ex, "Handled exception: {Message}", ex.Message);
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

        private void HandleException(CompanyIsNotActiveException exception)
        {
            Type = "https://example.com/probs/Conflict";
            Title = exception.Message;
            Status = StatusCodes.Status409Conflict;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(CompanyNotFoundException exception)
        {
            Type = "https://example.com/probs/Not-Found";
            Title = exception.Message;
            Status = StatusCodes.Status404NotFound;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(UserNotFoundException exception)
        {
            Type = "https://example.com/probs/Not-Found";
            Title = exception.Message;
            Status = StatusCodes.Status404NotFound;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(AccountAlreadyExistsException exception)
        {
            Type = "https://example.com/probs/Conflict";
            Title = exception.Message;
            Status = StatusCodes.Status409Conflict;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(AccountCouldNotActivateException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
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

        private void HandleException(AccountCouldNotDepositException exception)
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

        private void HandleException(AccountCouldNotWithdrawException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
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

        private void HandleException(CategoryNotFoundException exception)
        {
            Type = "https://example.com/probs/Not-Found";
            Title = exception.Message;
            Status = StatusCodes.Status404NotFound;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(DepositCouldNotValidateException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            Errors = exception.Errors;
            LogLevel = LogLevel.Information;
        }

        private void HandleException(InsufficientFundsException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(OfferAccessDeniedException exception)
        {
            Type = "https://example.com/probs/Forbidden";
            Title = exception.Message;
            Status = StatusCodes.Status403Forbidden;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(OfferCouldNotBeCreatedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(OfferCouldNotBeDeletedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(OfferCouldNotDecreaseStockException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(OfferCouldNotIncreaseStockException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(OfferCouldNotValidateException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            Errors = exception.Errors;
            LogLevel = LogLevel.Information;
        }

        private void HandleException(OfferExpiredException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(OfferNotFoundException exception)
        {
            Type = "https://example.com/probs/Not-Found";
            Title = exception.Message;
            Status = StatusCodes.Status404NotFound;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(RefundFailedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(InvalidTokenException exception)
        {
            Type = "https://example.com/probs/Unauthorized";
            Title = exception.Message;
            Status = StatusCodes.Status401Unauthorized;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(TransactionAccessDeniedException exception)
        {
            Type = "https://example.com/probs/Forbidden";
            Title = exception.Message;
            Status = StatusCodes.Status403Forbidden;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(TransactionCouldNotBeCreatedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(TransactionCouldNotBeRefundedException exception)
        {
            Type = "https://example.com/probs/Server-Error";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }

        private void HandleException(TransactionCouldNotValidateException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            Errors = exception.Errors;
            LogLevel = LogLevel.Information;
        }

        private void HandleException(TransactionInvalidStockException exception)
        {
            Type = "https://example.com/probs/Bad-Request";
            Title = exception.Message;
            Status = StatusCodes.Status400BadRequest;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(TransactionNotFoundException exception)
        {
            Type = "https://example.com/probs/Not-Found";
            Title = exception.Message;
            Status = StatusCodes.Status404NotFound;
            LogLevel = LogLevel.Warning;
        }

        private void HandleException(Exception exception)
        {
            Type = "https://example.com/probs/Unknown";
            Title = exception.Message;
            Status = StatusCodes.Status500InternalServerError;
            LogLevel = LogLevel.Error;
        }
    }
}
