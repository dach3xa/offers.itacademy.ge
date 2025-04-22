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
    public class GetCompanyHandler : IRequestHandler<GetCompanyQuery, CompanyResponseModel>
    {
        private readonly IAccountService _accountService;

        public GetCompanyHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<CompanyResponseModel> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
        {
            var companyResponse = await _accountService.GetCompanyAsync(request.Id, cancellationToken);
            return companyResponse;
        }
    }
}
