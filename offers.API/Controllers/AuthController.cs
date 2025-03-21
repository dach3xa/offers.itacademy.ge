using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using offers.API.Models.AppUserDTO;
using offers.API.Models.CompanyDTO;
using offers.API.Models.UserDTO;

namespace offers.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AuthController
    {
        private readonly IAppUserService _appUserService;
        private readonly IOptions<JWTConfiguration> _options;


        public UserController(IAppUserService appUserService, IOptions<JWTConfiguration> options)
        {
            _appUserService = appUserService;
            _options = options;
        }

        [Route("User/register")]
        [HttpPost]
        public async Task<string> Register(UserRegisterDTO userDTO, CancellationToken cancellation = default)
        {
            var user = userDTO.Adapt<AppUser>();
            var result = await _appUserService.CreateAsync(user, cancellation);

            return result;
        }

        [Route("Company/register")]
        [HttpPost]
        public async Task<string> Register(CompanyRegisterDTO companyDTO, CancellationToken cancellation = default)
        {
            var company = companyDTO.Adapt<AppUser>();
            var result = await _appUserService.CreateAsync(company, cancellation);

            return result;
        }

        [Route("login")]
        [HttpPost]
        public async Task<string> LogIn(AppUserLoginDTO appUserDTO, CancellationToken cancellation = default)
        {
            var result = await _appUserService.AuthenticationAsync(appUserDTO.Email, appUserDTO.Password, cancellation);

            return JWTHelper.GenerateSecurityToken(result.Email, result.Role, _options);
        }
    }
}
