using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Admin;
using offers.Application.Services.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Admin
{
    public class ConfirmCompanyHandler : IRequestHandler<ConfirmCompanyCommand, Unit>
    {
        private readonly IAccountService _accountService;

        public ConfirmCompanyHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<Unit> Handle(ConfirmCompanyCommand request, CancellationToken cancellationToken)
        {
            await _accountService.ConfirmCompanyAsync(request.Id, cancellationToken);
            return Unit.Value;
        }
    }
}
