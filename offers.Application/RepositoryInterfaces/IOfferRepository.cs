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
        Task CreateAsync(Offer offer, CancellationToken cancellationToken);
        Task<List<Offer>> GetOffersByAccountIdAsync(int accountId, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<Offer?> GetAsync(int id, CancellationToken cancellationToken);
        Task<List<Offer>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        void Delete(Offer offer);
        Task<List<Offer>> GetOffersByCategoriesAsync(List<int> categoryIds, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task DecreaseStockAsync(int id, int count, CancellationToken cancellationToken);
        Task IncreaseStockAsync(int id, int count, CancellationToken cancellationToken);
        Task ArchiveOffersAsync(CancellationToken cancellationToken);
        Task ChangePictureAsync(int accountId, string newPhotoUrl, CancellationToken cancellationToken);

    }
}
