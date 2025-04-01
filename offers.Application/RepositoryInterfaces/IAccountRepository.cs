using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.RepositoryInterfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetAsync(string Email, CancellationToken cancellationToken);
        Task<Account?> GetAsync(int id, CancellationToken cancellationToken);
        Task<bool> Exists(string Email, CancellationToken cancellationToken);
        Task RegisterAsync(Account account, CancellationToken cancellationToken);
        Task<List<Account>> GetAllCompaniesAsync(CancellationToken cancellationToken);
        Task<List<Account>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken);
        Task WithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken);
        Task DepositAsync(int  accountId, decimal amount, CancellationToken cancellationToken);
    }
}
