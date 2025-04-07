using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Models.DTO;
using offers.Application.Models.ViewModel;
using offers.Application.Services.Accounts;
using offers.Domain.Enums;
using offers.Domain.Models;
using offers.Web.Controllers.FileSaver;
using System.Data;
using System.Security.Claims;

namespace offers.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AccountController(UserManager<Account> userManager, SignInManager<Account> signInManager, IAccountService accountService, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
            _webHostEnvironment = webHostEnvironment;
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

            var user = await _accountService.LoginMvcAsync(request.Email, request.Password, cancellationToken);

            return RedirectToAction("Home", user.Role.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCompany([FromForm] CompanyRegisterViewModel companyRegister, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View();

            string photoUrl = null;
            try
            {
                if (companyRegister.Photo != null && companyRegister.Photo.Length > 0)
                {
                    photoUrl = await UploadedFileSaver.SaveUploadedFileAsync(companyRegister.Photo, _webHostEnvironment);
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("Photo", "Only image files are allowed.");
                return View(companyRegister);
            }

            var account = companyRegister.Adapt<Account>();

            account.CompanyDetail.PhotoURL = photoUrl;

            await _accountService.RegisterAsync(account, cancellationToken);

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
