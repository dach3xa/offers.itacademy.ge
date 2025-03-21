using FluentValidation;
using offers.API.Models.AppUserDTO;

namespace offers.API.Infrastructure.Validators
{
    public class AppUserLoginDTOValidator : AbstractValidator<AppUserLoginDTO>
    {
        public AppUserLoginDTOValidator()
        {
            RuleFor(x => x.Email)
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
