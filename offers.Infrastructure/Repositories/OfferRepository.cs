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
                .Where(off => off.ArchiveAt <= DateTime.UtcNow)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var offer in offersToArchive)
            {
                offer.IsArchived = true;
            }
        }

        public async Task DecreaseStockAsync(int id, int count, CancellationToken cancellationToken)
        {
            var offer = await _dbSet.FindAsync(id, cancellationToken).ConfigureAwait(false);
            offer.Count -= count;
        }

        public async Task<List<Offer>> GetOffersByAccountIdAsync(int accountId,int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(off => off.Category)
                .Where(off => off.AccountId == accountId && off.Count != 0)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Offer?> GetAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(off => off.Category)
                .SingleOrDefaultAsync(off => off.Id == id && off.Count != 0, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Offer>> GetOffersByCategoriesAsync(List<int> categoryIds, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(off => categoryIds.Contains(off.CategoryId) && off.IsArchived == false && off.Count != 0)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task IncreaseStockAsync(int id, int count, CancellationToken cancellationToken)
        {
            var offer = await _dbSet.FindAsync(id, cancellationToken).ConfigureAwait(false);
            offer.Count += count;
        }

        public async Task CreateAsync(Offer offer, CancellationToken cancellationToken)
        {
            await base.CreateAsync(offer, cancellationToken);
        }

        public void Delete(Offer offer)
        {
             base.Delete(offer);
        }
        public async Task<List<Offer>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _dbSet.Include(off => off.Category)
                .Where(off => !off.IsArchived && off.Count != 0)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
