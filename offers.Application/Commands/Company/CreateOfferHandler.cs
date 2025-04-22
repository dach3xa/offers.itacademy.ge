using MediatR;
using offers.Application.Commands.Admin;
using offers.Application.Models.Response;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Company
{
    public class CreateOfferHandler : IRequestHandler<CreateOfferCommand, OfferResponseModel>
    {
        private readonly IOfferService _offerService;

        public CreateOfferHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<OfferResponseModel> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
        {
            var offerResponse = await _offerService.CreateAsync(request.Offer, cancellationToken);
            return offerResponse;
        }
    }
}
