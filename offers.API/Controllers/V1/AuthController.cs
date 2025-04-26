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
using Microsoft.AspNetCore.Hosting;
using offers.Application.FileSaver;
using MediatR;
using offers.Application.Commands.Auth;

namespace offers.API.Controllers.V1
{
    [ApiController]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class AuthController : ControllerBase
    {
        private readonly IOptions<JWTConfiguration> _options;
        private readonly ILogger<AuthController> _logger;
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator, IOptions<JWTConfiguration> options, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user account.
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseModel))]
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
            var userResponse = await _mediator.Send(new RegisterUserCommand(userAccount), cancellation);

            return Ok(userResponse);
        }

        /// <summary>
        /// Registers a new company account.
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
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [SwaggerRequestExample(typeof(CompanyRegisterDTO), typeof(CompanyRegisterDTOMultipleExamples))]
        [HttpPost("company/register")]
        public async Task<IActionResult> Register([FromForm] CompanyRegisterDTO companyDTO, CancellationToken cancellation = default)
        {
            ControllerHelper.ValidateModelState(
            ModelState,
            errors => throw new AccountCouldNotValidateException("Could not validate the given Account", errors));

            _logger.LogInformation("Register attempt for {Email}", companyDTO.Email);
            string photoUrl = "";
            if (companyDTO.Photo != null )
            {
                photoUrl = await UploadedFileSaver.SaveUploadedFileAsync(companyDTO.Photo, cancellation);
            }
            else
            {
                photoUrl = "/uploads/company-placeholder.jpg";
            }
            var companyAccount = companyDTO.Adapt<Account>();
            companyAccount.CompanyDetail.PhotoURL = photoUrl;
            var companyResponse = await _mediator.Send(new RegisterCompanyCommand(companyAccount), cancellation);

            return Ok(companyResponse);

        }

        /// <summary>
        /// Authenticates an account and returns a JWT token.
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDTO))]
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
            var accountResponse = await _mediator.Send(new LoginCommand(accountLoginDTO.Email, accountLoginDTO.Password), cancellation);
            var token = JWTHelper.GenerateSecurityToken(accountResponse.Email, accountResponse.Id, accountResponse.Role, _options);
            return Ok(new LoginResponseDTO
            {
                Token = token,
                Email = accountResponse.Email,
                Role = accountResponse.Role,
                RoleName = accountResponse.Role.ToString(),
                Id = accountResponse.Id
            });
        }
    }
}
