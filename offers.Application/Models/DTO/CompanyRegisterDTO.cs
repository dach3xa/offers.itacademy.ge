using Microsoft.AspNetCore.Http;

namespace offers.Application.Models.DTO
{
    public class CompanyRegisterDTO
    {
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public IFormFile? Photo { get; set; }

    }
}
