using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Transactions
{
    public interface ITransactionService
    {
        public Task CreateAsync(Transaction transaction, CancellationToken cancellationToken);
    }
}
