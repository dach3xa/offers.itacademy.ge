using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Models.DTO;
using offers.Application.Services.Accounts;
using offers.Domain.Enums;
using offers.Domain.Models;
using System.Data;
using System.Security.Claims;

namespace offers.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly IAccountService _accountService;
        public AccountController(UserManager<Account> userManager, SignInManager<Account> signInManager, IAccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult RegisterCompany()
        {
            return View();
        }

        public IActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] AccountLoginDTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View();

            var user = await _accountService.LoginAsync(request.Email, request.Password, cancellationToken);

            return RedirectToAction("Home", user.Role.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCompany([FromForm] CompanyRegisterDTO companyDTO, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return View();

            var companyAccount = companyDTO.Adapt<Account>();
            var companyResponse = await _accountService.RegisterAsync(companyAccount, cancellation);

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromForm] UserRegisterDTO userDTO, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return View();

            var userAccount = userDTO.Adapt<Account>();
            var userResponse = await _accountService.RegisterAsync(userAccount, cancellation);

            return RedirectToAction(nameof(Login));
        }

    }
}
