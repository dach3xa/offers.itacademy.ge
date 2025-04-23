using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Guest;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.User
{
    public class GetOffersByCategoriesHandler : IRequestHandler<GetOffersByCategoriesQuery, List<OfferResponseModel>>
    {
        private readonly IOfferService _offerService;

        public GetOffersByCategoriesHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<List<OfferResponseModel>> Handle(GetOffersByCategoriesQuery request, CancellationToken cancellationToken)
        {
            var offerResponses = await _offerService.GetOffersByCategoriesAsync(request.CategoryIds, request.PageNumber, request.PageSize, cancellationToken);
            return offerResponses;
        }
    }
}
