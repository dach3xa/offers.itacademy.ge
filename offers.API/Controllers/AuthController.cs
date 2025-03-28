﻿using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using offers.API.Infrastructure.Auth.JWT;
using FluentValidation.AspNetCore;
using offers.Application.Exceptions;
using System.Security.Principal;
using offers.Domain.Models;
using offers.Application.Exceptions.Account;
using offers.API.Models;
using offers.Application.Services.Accounts;
using offers.API.Controllers.Helper;
using Azure;

namespace offers.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
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

        [HttpPost("user/register")]
        public async Task<IActionResult> Register(UserRegisterDTO userDTO, CancellationToken cancellation = default)
        {
            ControllerHelper.ValidateModelState(
            ModelState,
            errors => throw new AccountCouldNotValidateException("Could not validate the given Account", errors));

            _logger.LogInformation("Register attempt for {Email}", userDTO.Email);

            var userAccount = userDTO.Adapt<Account>();
            var userResponse = await _accountService.RegisterAsync(userAccount, cancellation);

            return CreatedAtAction(nameof(UserController.GetCurrentUser), "User", null, userResponse);
        }

        [HttpPost("company/register")]
        public async Task<IActionResult> Register(CompanyRegisterDTO companyDTO, CancellationToken cancellation = default)
        {
            ControllerHelper.ValidateModelState(
            ModelState,
            errors => throw new AccountCouldNotValidateException("Could not validate the given Account", errors));

            _logger.LogInformation("Register attempt for {Email}", companyDTO.Email);

            var companyAccount = companyDTO.Adapt<Account>();
            var companyResponse = await _accountService.RegisterAsync(companyAccount, cancellation);

            return CreatedAtAction(nameof(CompanyController.GetCurrentCompany), "Company", null, companyResponse);

        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn(AccountLoginDTO accountLoginDTO, CancellationToken cancellation = default)
        {
            ControllerHelper.ValidateModelState(
            ModelState,
            errors => throw new AccountCouldNotValidateException("Could not validate the given Account", errors));

            _logger.LogInformation("Login attempt for {Email}", accountLoginDTO.Email);

            var accountResponse = await _accountService.LoginAsync(accountLoginDTO.Email, accountLoginDTO.Password, cancellation);
            var token = JWTHelper.GenerateSecurityToken(accountResponse.Email, accountResponse.Id, accountResponse.Role, _options);
            return Ok(new LoginResponseDTO
            {
                Token = token,
                Email = accountResponse.Email,
                Role = accountResponse.Role
            });
        }
    }
}
