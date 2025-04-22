using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Admin;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Company
{
    public record GetMyOffersQuery(int AccountId, int PageNumber, int PageSize) : IRequest<List<OfferResponseModel>>
    {
    }
}
