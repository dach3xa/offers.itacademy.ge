using System.ComponentModel.DataAnnotations;

namespace offers.Application.Models.DTO
{
    public class DepositRequestDTO
    {
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }
}
