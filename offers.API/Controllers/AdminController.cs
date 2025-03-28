using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using offers.API.Controllers.Helper;
using offers.API.Infrastructure.Auth.JWT;
using offers.API.Models;
using offers.Application.Exceptions;
using offers.Application.Exceptions.Category;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Domain.Enums;
using offers.Domain.Models;

namespace offers.API.Controllers
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.Admin))]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IAccountService _accountService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAccountService accountService, ICategoryService categoryService, ILogger<AdminController> logger)
        {
            _accountService = accountService;
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CategoryDTO categoryDTO, CancellationToken cancellation)
        {
            ControllerHelper.ValidateModelState(
            ModelState,
            errors => throw new CategoryCouldNotValidateException("Could not validate the given category", errors));

            _logger.LogInformation("attempt to add a new category {Name}", categoryDTO.Name);

            var category = categoryDTO.Adapt<Category>();
            await _categoryService.CreateAsync(category, cancellation);

            return StatusCode(201);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellation)
        {
            var users = await _accountService.GetAllUsersAsync(cancellation);

            return Ok(users);
        }

        [HttpPatch("companies/{id}/confirm")]
        public async Task<IActionResult> ConfirmCompany(int id, CancellationToken cancellation)
        {
            await _accountService.ConfirmCompanyAsync(id, cancellation);

            return NoContent();
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies(CancellationToken cancellation)
        {
            var companies = await _accountService.GetAllCompaniesAsync(cancellation);

            return Ok(companies);
        }
    }
}
