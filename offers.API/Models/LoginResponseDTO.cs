using offers.Domain.Enums;

namespace offers.API.Models
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public AccountRole Role { get; set; }
    }
}
