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
    public class GetAllCompaniesHandler : IRequestHandler<GetAllCompaniesQuery, List<CompanyResponseModel>>
    {
        private readonly IAccountService _accountService;

        public GetAllCompaniesHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<List<CompanyResponseModel>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
        {
            var companyResponses = await _accountService.GetAllCompaniesAsync(request.PageNumber, request.PageSize, cancellationToken);
            return companyResponses;
        }
    }
}
