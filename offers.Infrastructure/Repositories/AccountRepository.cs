using Microsoft.EntityFrameworkCore;
using offers.Application.RepositoryInterfaces;
using offers.Domain.Enums;
using offers.Domain.Models;
using offers.Infrastructure.Repositories.Base;
using offers.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Infrastructure.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(ApplicationDbContext context) : base(context) { }

        public async Task ChangePictureAsync(int accountId, string newPhotoUrl, CancellationToken cancellationToken)
        {
            var companyAccount = await _dbSet.Include(acc => acc.CompanyDetail).SingleOrDefaultAsync(acc => acc.Id == accountId, cancellationToken);
            companyAccount.CompanyDetail.PhotoURL = newPhotoUrl;
        }

        public async Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken)
        {
            var account = await _dbSet.Include(acc => acc.CompanyDetail).SingleOrDefaultAsync(acc => acc.Id == id, cancellationToken).ConfigureAwait(false);
            account.CompanyDetail.IsActive = true;
        }

        public async Task DepositAsync(int accountId, decimal amount, CancellationToken cancellationToken)
        {
            var account = await _dbSet.Include(acc => acc.UserDetail).SingleOrDefaultAsync(acc => acc.Id == accountId, cancellationToken).ConfigureAwait(false); ;
            account.UserDetail.Balance += amount;
        }

        public async Task<bool> ExistsAsync(string Email, CancellationToken cancellationToken)
        {
            return await base.AnyAsync(acc => acc.Email == Email, cancellationToken);
        }

        public async Task<List<Account>> GetAllCompaniesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(acc => acc.CompanyDetail)
                .Where(acc => acc.Role == AccountRole.Company)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Account>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(acc => acc.UserDetail)
                .Where(acc => acc.Role == AccountRole.User)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false); ;
        }

        public async Task<Account?> GetAsync(string Email, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(acc => acc.UserDetail)
                .Include(acc => acc.CompanyDetail)
                .SingleOrDefaultAsync(acc => acc.Email == Email, cancellationToken).ConfigureAwait(false); ;
        }

        public async Task<Account?> GetAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(acc => acc.UserDetail)
                .Include(acc => acc.CompanyDetail)
                .FirstOrDefaultAsync(acc => acc.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task RegisterAsync(Account account, CancellationToken cancellationToken)
        {
            await base.CreateAsync(account, cancellationToken);
        }

        public async Task WithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken)
        {
            var account = await _dbSet.Include(acc => acc.UserDetail).SingleOrDefaultAsync(acc => acc.Id == accountId, cancellationToken).ConfigureAwait(false); ;
            account.UserDetail.Balance -= amount;
        }
    }
}
