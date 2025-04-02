using offers.Application.RepositoryInterfaces;
using offers.Infrastructure.Repositories.Base;
using offers.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using offers.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace offers.Infrastructure.Repositories
{
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context) { }

        public async Task CreateAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            await base.CreateAsync(transaction, cancellationToken);
        }

        public async Task DeleteByOfferIdAsync(int offerId, CancellationToken cancellationToken)
        {
            var transToDelete = await _dbSet
            .Where(tran => tran.OfferId == offerId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

            _dbSet.RemoveRange(_dbSet.Where(tran => tran.OfferId == offerId));
        }

        public async Task<List<Transaction>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken)
        {
            return await _dbSet
            .Where(tran => tran.OfferId == offerId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await base.DeleteAsync(id, cancellationToken);
        }

        public async Task<Transaction?> GetAsync(int id, CancellationToken cancellationToken)
        {
           return await _dbSet
                .Include(tran => tran.Offer)
                .Include(tran => tran.User)
                .SingleOrDefaultAsync(tran => tran.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Transaction>> GetMyTransactionsAsync(int accountId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(tran => tran.Offer)
                .Include(tran => tran.User)
                .Where(tran => tran.UserId == accountId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
