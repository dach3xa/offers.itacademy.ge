using offers.Application.Models.Response;
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
        public Task<OfferResponseModel> CreateAsync(Offer offer, CancellationToken cancellationToken);
        public Task<List<OfferResponseModel>> GetMyOffersAsync(int accountId, CancellationToken cancellationToken);
        public Task<List<OfferResponseModel>> GetAllAsync(CancellationToken cancellationToken);
        public Task<OfferResponseModel> GetAsync(int id, CancellationToken cancellationToken);
        public Task<OfferResponseModel> GetMyOfferAsync(int id, int accountId, CancellationToken cancellationToken);
        public Task<List<OfferResponseModel>> GetOffersByCategoriesAsync(List<int> categoryIds, CancellationToken cancellationToken);   
        public Task DeleteAsync(int id, int accountId, CancellationToken cancellationToken);
        public Task DecreaseStockAsync(int id, int count, CancellationToken cancellationToken);
        public Task IncreaseStockAsync(int id, int count, CancellationToken cancellationToken);
        public Task ArchiveOffersAsync(CancellationToken cancellationToken);
    }
}
