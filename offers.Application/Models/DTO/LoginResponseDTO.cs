using offers.Domain.Enums;

namespace offers.Application.Models.DTO
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public AccountRole Role { get; set; }
    }
}
