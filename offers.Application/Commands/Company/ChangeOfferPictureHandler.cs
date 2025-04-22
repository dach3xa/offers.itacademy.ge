using MediatR;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Company
{
    public class ChangeOfferPictureHandler : IRequestHandler<ChangeOfferPictureCommand, Unit>
    {
        private readonly IOfferService _offerService;

        public ChangeOfferPictureHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<Unit> Handle(ChangeOfferPictureCommand request, CancellationToken cancellationToken)
        {
            await _offerService.ChangePictureAsync(request.Id, request.AccountId, request.NewPhotoURL, cancellationToken);
            return Unit.Value;
        }
    }
}
