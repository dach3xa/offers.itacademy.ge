using Microsoft.EntityFrameworkCore;
using offers.Application.RepositoryInterfaces;
using offers.Domain.Enums;
using offers.Domain.Models;
using offers.Infrastructure.Repositories;
using offers.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Infrastructure.Repos
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(ApplicationDbContext context) : base(context) { }

        public async Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken)
        {
            var account = await _dbSet.FindAsync(id, cancellationToken);
            account.CompanyDetail.IsActive = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Account?> DepositAsync(int accountId, decimal amount, CancellationToken cancellationToken)
        {
            var account = await _dbSet.FindAsync(accountId, cancellationToken);
            account.UserDetail.Balance += amount;
            await _context.SaveChangesAsync(cancellationToken);

            return account;
        }

        public async Task<bool> Exists(string Email, CancellationToken cancellationToken)
        {
            return await base.AnyAsync(acc => acc.Email == Email, cancellationToken);
        }

        public async Task<List<Account>> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.Where(acc => acc.Role == AccountRole.Company).ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.Where(acc => acc.Role == AccountRole.User).ToListAsync(cancellationToken);
        }

        public async Task<Account?> GetAsync(string Email, CancellationToken cancellationToken)
        {
            return await _dbSet.SingleOrDefaultAsync(acc => acc.Email == Email, cancellationToken);
        }

        public Task<Account?> GetAsync(int id, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, cancellationToken);
        }

        public async Task<Account?> RegisterAsync(Account account, CancellationToken cancellationToken)
        {
            await base.CreateAsync(account, cancellationToken);

            return await GetAsync(account.Id, cancellationToken);
        }

        public async Task<Account?> WithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken)
        {
            var account = await _dbSet.FindAsync(accountId, cancellationToken);
            account.UserDetail.Balance -= amount;
            await _context.SaveChangesAsync(cancellationToken);

            return account;
        }
    }
}
