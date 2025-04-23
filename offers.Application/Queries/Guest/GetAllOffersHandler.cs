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
    public class GetAllOffersHandler : IRequestHandler<GetAllOffersQuery, List<OfferResponseModel>>
    {
        private readonly IOfferService _offerService;

        public GetAllOffersHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<List<OfferResponseModel>> Handle(GetAllOffersQuery request, CancellationToken cancellationToken)
        {
            var offerResponses = await _offerService.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);
            return offerResponses;
        }
    }
}
