﻿using FluentValidation;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;

namespace offers.Application.Validators
{
    public class UserRegisterDTOValidator : AbstractValidator<UserRegisterDTO>
    {
        public UserRegisterDTOValidator()
        {
            RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .Must(CompanyName => !string.IsNullOrWhiteSpace(CompanyName)).WithMessage("first name is required")
            .MaximumLength(100).WithMessage("First Name must not exceed 100 characters");

            RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .Must(CompanyName => !string.IsNullOrWhiteSpace(CompanyName)).WithMessage("Last Name is required")
            .MaximumLength(100).WithMessage("Last Name must not exceed 100 characters");

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
        }
    }
}
