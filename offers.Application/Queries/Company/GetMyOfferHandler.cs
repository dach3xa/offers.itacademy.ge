using MediatR;
using offers.Application.Models.Response;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Company
{
    public class GetMyOfferHandler : IRequestHandler<GetMyOfferQuery, OfferResponseModel>
    {
        private readonly IOfferService _offerService;

        public GetMyOfferHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<OfferResponseModel> Handle(GetMyOfferQuery request, CancellationToken cancellationToken)
        {
            var offerResponse = await _offerService.GetMyOfferAsync(request.Id, request.AccountId, cancellationToken);
            return offerResponse;
        }
    }
}
