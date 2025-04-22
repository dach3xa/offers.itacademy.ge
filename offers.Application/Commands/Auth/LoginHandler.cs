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
    public class LoginHandler : IRequestHandler<LoginCommand, AccountResponseModel>
    {
        private readonly IAccountService _accountService;

        public LoginHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<AccountResponseModel> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var accountResponse = await _accountService.LoginAsync(request.Email, request.Password, cancellationToken);
            return accountResponse;
        }
    }
}
