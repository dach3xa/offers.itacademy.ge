using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Admin;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Company
{
    public class GetMyOffersHandler : IRequestHandler<GetMyOffersQuery, List<OfferResponseModel>>
    {
        private readonly IOfferService _offerService;

        public GetMyOffersHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<List<OfferResponseModel>> Handle(GetMyOffersQuery request, CancellationToken cancellationToken)
        {
            var offerResponses = await _offerService.GetMyOffersAsync(request.AccountId, request.PageNumber, request.PageSize, cancellationToken);
            return offerResponses;
        }
    }
}
