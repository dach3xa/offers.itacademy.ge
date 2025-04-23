using MediatR;
using offers.Application.Commands.Company;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.User
{
    public class DepositHandler : IRequestHandler<DepositCommand, Unit>
    {
        private readonly IAccountService _accountService;

        public DepositHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<Unit> Handle(DepositCommand request, CancellationToken cancellationToken)
        {
            await _accountService.DepositAsync(request.AccountId, request.Ammount, cancellationToken);
            return Unit.Value;
        }
    }
}
