using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Offers
{
    public interface IOfferService
    {
        public Task CreateAsync(Offer offer, CancellationToken cancellationToken);
        public Task<List<Offer>> GetMyOffersAsync(int accoundId, CancellationToken cancellationToken);
    }
}
