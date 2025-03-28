using Microsoft.AspNetCore.Mvc.ModelBinding;
using offers.Application.Exceptions.Token;
using System.Security.Claims;

namespace offers.API.Controllers.Helper
{
    public static class ControllerHelper
    {
        public static void ValidateModelState(ModelStateDictionary modelState, Func<List<string>, Exception> exceptionFactory)
        {
            if (!modelState.IsValid) 
            {
                var errors = modelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                exceptionFactory(errors);
            }            
        }

        public static int GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var accountIdString = user.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(accountIdString) || !int.TryParse(accountIdString, out var accountId))
            {
                throw new InvalidTokenException("Invalid or missing account Id");
            }

            return accountId;
        }
    }
}
