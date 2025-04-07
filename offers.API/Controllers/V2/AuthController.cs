using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using offers.API.Infrastructure.Auth.JWT;
using FluentValidation.AspNetCore;
using offers.Application.Exceptions;
using System.Security.Principal;
using offers.Domain.Models;
using offers.Application.Exceptions.Account;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using offers.Application.Services.Accounts;
using offers.Application.Helper;
using Azure;
using offers.API.Infrastructure.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;
using Asp.Versioning;
using offers.API.Infrastructure.Middlewares;

namespace offers.API.Controllers.V2
{
    [ApiController]
    [AllowAnonymous]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IOptions<JWTConfiguration> _options;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAccountService accountService, IOptions<JWTConfiguration> options, ILogger<AuthController> logger)
        {
            _accountService = accountService;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user account for version 2.
        /// </summary>
        /// <param name="userDTO">The user registration data.</param>
        /// <returns>
        /// A 201 Created response with the registered user data,
        /// or an error response if the registration fails.
        /// </returns>
        /// <response code="201">User account created successfully</response>
        /// <response code="400">Validation failed (AccountCouldNotValidateException)</response>
        /// <response code="409">An account with the given email already exists (AccountAlreadyExistsException)</response>
        /// <response code="500">Internal server error during registration (AccountCouldNotBeCreatedException)</response>
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [SwaggerRequestExample(typeof(UserRegisterDTO), typeof(UserRegisterDTOMultipleExamples))]
        [HttpPost("user/register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userDTO, CancellationToken cancellation = default)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => throw new AccountCouldNotValidateException("Could not validate the given Account", errors));

            _logger.LogInformation("Register attempt for {Email}", userDTO.Email);

            var userAccount = userDTO.Adapt<Account>();
            var userResponse = await _accountService.RegisterAsync(userAccount, cancellation);

            var responseV2 = new
            {
                userResponse,
                message = "User registered successfully in v2",
                timestamp = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(UserController.GetCurrentUser), "User", null, responseV2);
        }

        /// <summary>
        /// Registers a new company account for version 2.
        /// </summary>
        /// <param name="companyDTO">The company registration data.</param>
        /// <returns>
        /// A 201 Created response with the registered company account,
        /// or an error response if the registration fails.
        /// </returns>
        /// <response code="201">Company account created successfully</response>
        /// <response code="400">Validation failed (AccountCouldNotValidateException)</response>
        /// <response code="409">An account with the given email already exists (AccountAlreadyExistsException)</response>
        /// <response code="500">Internal server error during registration</response>
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [SwaggerRequestExample(typeof(CompanyRegisterDTO), typeof(CompanyRegisterDTOMultipleExamples))]
        [HttpPost("company/register")]
        public async Task<IActionResult> Register([FromBody] CompanyRegisterDTO companyDTO, CancellationToken cancellation = default)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => throw new AccountCouldNotValidateException("Could not validate the given Account", errors));

            _logger.LogInformation("Register attempt for {Email}", companyDTO.Email);

            var companyAccount = companyDTO.Adapt<Account>();
            var companyResponse = await _accountService.RegisterAsync(companyAccount, cancellation);

            var responseV2 = new
            {
                companyResponse,
                companyName = companyDTO.CompanyName,  
                message = "Company registered successfully in v2"
            };

            return CreatedAtAction(nameof(CompanyController.GetCurrentCompany), "Company", null, responseV2);
        }

        /// <summary>
        /// Authenticates an account and returns a JWT token for version 2.
        /// </summary>
        /// <param name="accountLoginDTO">The login credentials (email and password).</param>
        /// <returns>
        /// A 200 OK response with a JWT token and account information,
        /// or an error response if authentication fails.
        /// </returns>
        /// <response code="200">Login successful, returns a JWT token and account info</response>
        /// <response code="400">Validation failed (AccountCouldNotValidateException)</response>
        /// <response code="404">Account not found or invalid credentials (AccountNotFoundException)</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [SwaggerRequestExample(typeof(AccountLoginDTO), typeof(AccountLoginDTOMultipleExamples))]
        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] AccountLoginDTO accountLoginDTO, CancellationToken cancellation = default)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => throw new AccountCouldNotValidateException("Could not validate the given Account", errors));

            _logger.LogInformation("Login attempt for {Email}", accountLoginDTO.Email);
            var accountResponse = await _accountService.LoginAsync(accountLoginDTO.Email, accountLoginDTO.Password, cancellation);
            var token = JWTHelper.GenerateSecurityToken(accountResponse.Email, accountResponse.Id, accountResponse.Role, _options);

            var responseV2 = new
            {
                Token = token,
                Email = accountResponse.Email,
                Role = accountResponse.Role,
                LoginTimestamp = DateTime.UtcNow 
            };

            return Ok(responseV2);
        }
    }
}
