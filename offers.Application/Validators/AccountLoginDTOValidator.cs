using FluentValidation;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;

namespace offers.Application.Validators
{
    public class AccountLoginDTOValidator : AbstractValidator<AccountLoginDTO>
    {
        public AccountLoginDTOValidator()
        {
            RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

            RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be a minimum length of 6")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Matches(@"^\S+$").WithMessage("Password must not contain spaces.");
        }
    }
}
