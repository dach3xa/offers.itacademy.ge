using MediatR;
using offers.Application.Models.Response;
using offers.Application.Services.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Auth
{
    public class RegisterCompanyHandler : IRequestHandler<RegisterCompanyCommand, AccountResponseModel>
    {
        private readonly IAccountService _accountService;

        public RegisterCompanyHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<AccountResponseModel> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
        {
            var companyResponse = await _accountService.RegisterAsync(request.Account, cancellationToken);
            return companyResponse;
        }
    }
}
