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
        Task<Transaction?> CreateAsync(Transaction transaction, CancellationToken cancellationToken);
        Task<List<Transaction>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken);
        Task<List<Transaction>?> DeleteByOfferIdAsync(int offerId, CancellationToken cancellationToken);
        Task<Transaction> GetAsync(int id, CancellationToken cancellationToken);
        Task<Transaction?> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
