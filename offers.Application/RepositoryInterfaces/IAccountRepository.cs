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
        Task<Account> GetAsync(string Email, CancellationToken cancellationToken);
        Task<Account> GetAsync(int id, CancellationToken cancellationToken);
        Task<bool> Exists(string Email, CancellationToken cancellationToken);
        Task<bool> RegisterAsync(Account account, CancellationToken cancellationToken);
        Task<bool> GetAllCompaniesAsync(CancellationToken cancellationToken);
        Task<bool> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<bool> ConfirmCompanyAsync(int id, CancellationToken cancellationToken);
        Task<bool> WithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken);
        Task<bool> DepositAsync(int  accountId, decimal amount, CancellationToken cancellationToken);
    }
}
