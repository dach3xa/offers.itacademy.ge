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

namespace offers.Application.Queries.Admin
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserResponseModel>>
    {
        private readonly IAccountService _accountService;

        public GetAllUsersHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<List<UserResponseModel>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var userResponses = await _accountService.GetAllUsersAsync(request.PageNumber, request.PageSize, cancellationToken);
            return userResponses;
        }
    }
}
