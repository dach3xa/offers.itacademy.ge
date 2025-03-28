using offers.Application.Models;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace offers.Application.Services.Offers
{
    public interface IOfferService
    {
        public Task CreateAsync(Offer offer, CancellationToken cancellationToken);
        public Task<List<OfferResponseModel>> GetMyOffersAsync(int accoundId, CancellationToken cancellationToken);
        public Task<List<OfferResponseModel>> GetOffersByCategoriesAsync(List<int> categoryIds, CancellationToken cancellationToken);   
        public Task DeleteAsync(int id, int accountId, CancellationToken cancellationToken);
        public Task DecreaseStockAsync(int id, int count, CancellationToken cancellationToken);
        public Task ArchiveOffersAsync(CancellationToken cancellationToken);
    }
}
