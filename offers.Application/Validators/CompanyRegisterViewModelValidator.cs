using FluentValidation;
using offers.Application.Models.DTO;
using offers.Application.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Validators
{

    public class CompanyRegisterViewModelValidator : AbstractValidator<CompanyRegisterViewModel>
    {
        public CompanyRegisterViewModelValidator()
        {
            RuleFor(x => x.CompanyName)
            .Must(CompanyName => !string.IsNullOrWhiteSpace(CompanyName)).WithMessage("field Company Name is required")
            .MaximumLength(100).WithMessage("Company Name must not exceed 100 characters");

            RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Phone number must be digits only and between 7 and 15 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
                .Matches(@"[^\w\d\s]").WithMessage("Password must contain at least one special character")
                .Matches(@"^\S+$").WithMessage("Password must not contain spaces");

            RuleFor(x => x.Photo)
                .NotEmpty().WithMessage("Photo is required");
        }
    }
}
