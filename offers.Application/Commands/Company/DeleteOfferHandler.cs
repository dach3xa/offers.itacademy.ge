using MediatR;
using offers.Application.Models.Response;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Company
{
    public class DeleteOfferHandler : IRequestHandler<DeleteOfferCommand, Unit>
    {
        private readonly IOfferService _offerService;

        public DeleteOfferHandler(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<Unit> Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
        {
            await _offerService.DeleteAsync(request.Id, request.AccountId, cancellationToken);
            return Unit.Value;
        }
    }
}
