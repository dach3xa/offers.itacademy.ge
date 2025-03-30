using MediatR;
using offers.Application.Services.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Offers.Events
{
    public class OfferDeletedEventHandler : INotificationHandler<OfferDeletedEvent>
    {
        private readonly ITransactionService _transactionService;

        public OfferDeletedEventHandler(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task Handle(OfferDeletedEvent notification, CancellationToken cancellationToken)
        {
            await _transactionService.RefundAllUsersByOfferIdAsync(notification.OfferId, cancellationToken);
        }
    }

}
