using MediatR;
using offers.Application.Models.Response;
using offers.Application.Services.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Admin
{
    public class GetUserHandler : IRequestHandler<GetUserQuery, UserResponseModel>
    {
        private readonly IAccountService _accountService;

        public GetUserHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<UserResponseModel> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var userResponse = await _accountService.GetUserAsync(request.Id, cancellationToken);
            return userResponse;
        }
    }
}
