using FluentValidation;
using offers.API.Models;

namespace offers.API.Infrastructure.Validators
{
    public class OfferDTOValidator : AbstractValidator<OfferDTO>
    {
        public OfferDTOValidator()
        {
            RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Name must not be empty or just whitespace.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

            RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .Must(CompanyName => !string.IsNullOrWhiteSpace(CompanyName)).WithMessage("Description can not be just white space!")
            .MaximumLength(200).WithMessage("description must not exceed 200 characters");

            RuleFor(x => x.Count)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Count can not be empty")
            .Must(Count => Count > 0).WithMessage("Description can not be just white space!")
            .LessThanOrEqualTo(200).WithMessage("can't sell more than a 200 products in a single offer");

            RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Price is unreasonably high");


            RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");
        }
    }
}
