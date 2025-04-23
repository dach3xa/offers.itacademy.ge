using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Company;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Guest
{
    public class GetOfferHandler : IRequestHandler<GetOfferQuery, OfferResponseModel>
    {
        private readonly IOfferService _offerService;

        public GetOfferHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<OfferResponseModel> Handle(GetOfferQuery request, CancellationToken cancellationToken)
        {
            var offerResponse = await _offerService.GetAsync(request.Id, cancellationToken);
            return offerResponse;
        }
    }
}
