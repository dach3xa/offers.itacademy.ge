using offers.Application.Models;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace offers.Application.Services.Accounts
{
    public interface IAccountService
    {
        Task<AccountResponseModel> LoginAsync(string username, string password, CancellationToken cancellationToken);
        Task<AccountResponseModel> RegisterAsync(Account account, CancellationToken cancellationToken);
        Task<List<AccountResponseModel>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<List<AccountResponseModel>> GetAllCompaniesAsync(CancellationToken cancellationToken);
        Task<AccountResponseModel> GetUserAsync(int id, CancellationToken cancellationToken);
        Task<AccountResponseModel> GetCompanyAsync(int id, CancellationToken cancellationToken);
        Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken);
        Task WithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken);
        Task DepositAsync(int accountId, decimal amount, CancellationToken cancellationToken);
    }
}
