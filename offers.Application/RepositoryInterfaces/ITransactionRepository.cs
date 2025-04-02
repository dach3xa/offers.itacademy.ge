using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.RepositoryInterfaces
{
    public interface ITransactionRepository
    {
        Task CreateAsync(Transaction transaction, CancellationToken cancellationToken);
        Task<List<Transaction>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken);
        Task DeleteByOfferIdAsync(int offerId, CancellationToken cancellationToken);
        Task<Transaction?> GetAsync(int id, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
        Task<List<Transaction>> GetMyTransactionsAsync(int accountId, CancellationToken cancellationToken);
    }
}
