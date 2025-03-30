using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Offers.Events
{
    public class OfferDeletedEvent : INotification
    {
        public int OfferId { get; }

        public OfferDeletedEvent(int offerId)
        {
            OfferId = offerId;
        }
    }
}
