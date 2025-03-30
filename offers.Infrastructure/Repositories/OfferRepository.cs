using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using offers.Application.RepositoryInterfaces;
using offers.Domain.Models;
using offers.Infrastructure.Repositories.Base;
using offers.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace offers.Infrastructure.Repositories
{
    public class OfferRepository : BaseRepository<Offer>, IOfferRepository
    {
        public OfferRepository(ApplicationDbContext context) : base(context) { }
        public async Task ArchiveOffersAsync(CancellationToken cancellationToken)
        {
            var offersToArchive = await _dbSet
                .Where(off => off.ArchiveAt <= DateTime.Now)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var offer in offersToArchive)
            {
                offer.IsArchived = true;
            }
        }

        public async Task<Offer?> DecreaseStockAsync(int id, int count, CancellationToken cancellationToken)
        {
            var offer = await _dbSet.FindAsync(id, cancellationToken).ConfigureAwait(false);
            offer.Count -= count;

            return offer;
        }

        public async Task<List<Offer>> GetOffersByAccountIdAsync(int accountId, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(off => off.AccountId == accountId).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<Offer?> GetAsync(int id, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, cancellationToken);
        }

        public async Task<List<Offer>> GetOffersByCategoriesAsync(List<int> categoryIds, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(off => categoryIds.Contains(off.CategoryId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Offer?> IncreaseStockAsync(int id, int count, CancellationToken cancellationToken)
        {
            var offer = await _dbSet.FindAsync(id, cancellationToken).ConfigureAwait(false);
            offer.Count += count;

            return offer;
        }

        public async Task CreateAsync(Offer offer, CancellationToken cancellationToken)
        {
            await base.CreateAsync(offer, cancellationToken);
        }

        public void Delete(Offer offer)
        {
             base.Delete(offer);
        }
    }
}
