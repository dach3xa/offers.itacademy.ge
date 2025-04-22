using MediatR;
using offers.Application.Commands.Admin;
using offers.Application.Models.Response;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Auth
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AccountResponseModel>
    {
        private readonly IAccountService _accountService;

        public RegisterUserHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<AccountResponseModel> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userResponse = await _accountService.RegisterAsync(request.Account, cancellationToken);
            return userResponse;
        }
    }
}
