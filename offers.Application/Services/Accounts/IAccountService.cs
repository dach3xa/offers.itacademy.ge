using offers.Application.Models.Response;
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
        Task<AccountResponseModel> LoginAsync(string email, string password, CancellationToken cancellationToken);
        Task<AccountResponseModel> RegisterAsync(Account account, CancellationToken cancellationToken);
        Task<List<UserResponseModel>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<List<CompanyResponseModel>> GetAllCompaniesAsync(CancellationToken cancellationToken);
        Task<UserResponseModel> GetUserAsync(int id, CancellationToken cancellationToken);
        Task<CompanyResponseModel> GetCompanyAsync(int id, CancellationToken cancellationToken);
        Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken);
        Task WithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken);
        Task DepositAsync(int accountId, decimal amount, CancellationToken cancellationToken);
    }
}
