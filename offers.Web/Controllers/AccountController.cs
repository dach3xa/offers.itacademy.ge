using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Models.DTO;
using offers.Web.Models;
using offers.Application.Services.Accounts;
using offers.Domain.Enums;
using offers.Domain.Models;
using offers.Application.FileSaver;
using System.Data;
using System.Security.Claims;

namespace offers.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService, IWebHostEnvironment webHostEnvironment)
        {
            _accountService = accountService;
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet("company/register")]
        public IActionResult RegisterCompany()
        {
            return View();
        }
        [HttpGet("user/register")]
        public IActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] AccountLoginDTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View();

            var user = await _accountService.LoginMvcAsync(request.Email, request.Password, cancellationToken);

            return RedirectToAction("Home", user.Role.ToString());
        }

        [HttpPost("company/register")]
        public async Task<IActionResult> RegisterCompany([FromForm] CompanyRegisterDTO companyRegister, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View();

            string photoUrl = null;
            if (companyRegister.Photo != null && companyRegister.Photo.Length > 0)
            {
                photoUrl = await UploadedFileSaver.SaveUploadedFileAsync(companyRegister.Photo, cancellationToken);
            }
            else
            {
                photoUrl = "/uploads/company-placeholder.jpg";
            }
        

            var account = companyRegister.Adapt<Account>();

            account.CompanyDetail.PhotoURL = photoUrl;

            await _accountService.RegisterAsync(account, cancellationToken);

            return RedirectToAction(nameof(Login));
        }

        [HttpPost("user/register")]
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
