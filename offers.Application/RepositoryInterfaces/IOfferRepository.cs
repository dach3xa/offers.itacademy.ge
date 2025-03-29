using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.RepositoryInterfaces
{
    public interface IOfferRepository
    {
        Task<Offer?> CreateAsync(Offer offer, CancellationToken cancellationToken);
        Task<List<Offer>> GetOffersByAccountIdAsync(int accountId, CancellationToken cancellationToken);
        Task<Offer> GetAsync(int id, CancellationToken cancellationToken);
        Task<Offer?> DeleteAsync(Offer offer, CancellationToken cancellationToken);
        Task<List<Offer>> GetOffersByCategoriesAsync(List<int> categoryIds, CancellationToken cancellationToken);
        Task<Offer?> DecreaseStockAsync(int id, int count, CancellationToken cancellationToken);
        Task<Offer?> IncreaseStockAsync(int id, int count, CancellationToken cancellationToken);
        Task ArchiveOffersAsync(CancellationToken cancellationToken);

    }
}
