using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Helper;
using offers.Application.Models.DTO;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Domain.Enums;
using offers.Domain.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace offers.Web.Controllers
{
    [Authorize(Roles = nameof(AccountRole.Admin))]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IAccountService _accountService;

        public AdminController(ICategoryService categoryService, IAccountService accountService)
        {
            _categoryService = categoryService;
            _accountService = accountService;
        }

        [HttpGet("home")]
        public IActionResult Home()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            ViewBag.Email = email;
            return View();
        }

        [HttpGet("categories/create")]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost("categories/create")]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryDTO categoryDTO, CancellationToken cancellationToken)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            var category = categoryDTO.Adapt<Category>();
            await _categoryService.CreateAsync(category, cancellationToken);

            return RedirectToAction("home");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1, CancellationToken cancellationToken = default)
        {
            var users = await _accountService.GetAllUsersAsync(pageNumber, pageSize, cancellationToken);
            ViewBag.currentPage = pageNumber;
            ViewBag.CanGoRight = users.Count == pageSize;
            return View(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
        {
            var user = await _accountService.GetUserAsync(id, cancellationToken);

            return View(user);
        }

        [HttpPost("companies/{id}/confirm")]//called from front end
        public async Task<IActionResult> ConfirmCompany(int id, CancellationToken cancellationToken)
        {
            await _accountService.ConfirmCompanyAsync(id, cancellationToken);

            return RedirectToAction("GetCompany", new { id });
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var companies =  await _accountService.GetAllCompaniesAsync(pageNumber, pageSize, cancellationToken);
            ViewBag.currentPage = pageNumber;
            ViewBag.CanGoRight = companies.Count == pageSize;

            return View(companies);
        }

        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompany(int id, CancellationToken cancellationToken)
        {
            var company = await _accountService.GetCompanyAsync(id, cancellationToken);

            return View(company);
        }

    }
}
