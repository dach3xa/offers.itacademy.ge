using FluentValidation;
using offers.API.Models;

namespace offers.API.Infrastructure.Validators
{
    public class TransactionDTOValidator : AbstractValidator<TransactionDTO>
    {
        public TransactionDTOValidator()
        {

            RuleFor(x => x.Count)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Count can not be empty")
            .Must(Count => Count > 0).WithMessage("Count must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("You can't buy more than 100 products in a single offer.");

            RuleFor(x => x.Paid)
            .GreaterThan(0).WithMessage("you have to pay more than 0 dollars")
            .LessThanOrEqualTo(100000).WithMessage("Paid amount is unreasonably high");

            RuleFor(x => x.OfferId)
            .GreaterThan(0).WithMessage("offer id is required");
        }
    }
}
