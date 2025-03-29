using offers.Application.Exceptions;
using offers.Application.Models;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using System.Security.Principal;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Funds;
using offers.Application.RepositoryInterfaces;
using Mapster;
using offers.Application.Exceptions.Account.User;
using offers.Application.UOF;

namespace offers.Application.Services.Accounts
{
    public class AccountService : IAccountService
    {
        const string SECRET_KEY = "Secret_Hashing_Key";
        private readonly IAccountRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IAccountRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountResponseModel> LoginAsync(string Email, string password, CancellationToken cancellationToken)
        {
            var hashPassword = GenerateHash(password);
            var account = await _repository.GetAsync(Email, cancellationToken);

            if (account == null)
            {
                throw new AccountNotFoundException("Email or password is incorrect");
            }

            return account.Adapt<AccountResponseModel>();
        }

        public async Task<AccountResponseModel> RegisterAsync(Account account, CancellationToken cancellationToken)
        {
            var exists = await _repository.Exists(account.Email, cancellationToken);

            if (exists)
            {
                
                throw new AccountAlreadyExistsException("There already is an account with this email");
            }

            account.PasswordHash = GenerateHash(account.PasswordHash);
            var registeredAccount = await _repository.RegisterAsync(account, cancellationToken);

            if (registeredAccount == null)
            {
                throw new AccountCouldNotBeCreatedException("Failed to create account because of an unknown issue");
            }

            return registeredAccount.Adapt<AccountResponseModel>();
        }

        public async Task<List<AccountResponseModel>> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            var companies = await _repository.GetAllCompaniesAsync(cancellationToken);


            return companies.Adapt<List<AccountResponseModel>>() ?? new List<AccountResponseModel>();
        }

        public async Task<List<AccountResponseModel>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _repository.GetAllUsersAsync(cancellationToken);

            return users.Adapt<List<AccountResponseModel>>() ?? new List<AccountResponseModel>();
        }

        public async Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken)
        {
            var companyAccount = await _repository.GetAsync(id, cancellationToken);

            if (companyAccount == null || companyAccount.CompanyDetail == null)
            {
                throw new AccountNotFoundException("an account with the provided id does not exist");
            }

            if (companyAccount.CompanyDetail.IsActive)
            {
                throw new CompanyAlreadyActiveException("an account with the provided id is already active");
            }

            await _repository.ConfirmCompanyAsync(id, cancellationToken);
        }

        public async Task WithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Withdrawal amount must be greater than zero.");
            }

            var account = await _repository.GetAsync(accountId, cancellationToken);
            if (account == null || account.UserDetail == null)
            {
                throw new AccountNotFoundException("Account with the provided id does not exist");
            }

            if (account.UserDetail.Balance < amount)
            {
                throw new InsufficientFundsException("this account doesn't have sufficient funds for the transaction");
            }

            var withdrawedAccount = await _repository.WithdrawAsync(accountId, amount, cancellationToken);
            if (account.UserDetail.Balance - withdrawedAccount.UserDetail.Balance != amount)
            {
                throw new AccountCouldNotWithdrawException($"Account with the id {accountId} could not withdraw becouse of an unknown reason");
            }
        }


        public async Task DepositAsync(int accountId, decimal amount, CancellationToken cancellationToken)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Deposit amount must be greater than zero.");
            }

            var account = await _repository.GetAsync(accountId, cancellationToken);
            if (account == null || account.UserDetail == null)
            {
                throw new AccountNotFoundException("Account with the provided id does not exist");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var depositedAccount = await _repository.DepositAsync(accountId, amount, cancellationToken);
                if (depositedAccount.UserDetail.Balance - account.UserDetail.Balance != amount)
                {
                    throw new AccountCouldNotDepositException($"Deposit to account ID {accountId} failed due to an unknown error.");
                }
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private string GenerateHash(string input)
        {
            using (SHA512 sha = SHA512.Create())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(input + SECRET_KEY);
                byte[] hashBytes = sha.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public async Task<AccountResponseModel> GetUserAsync(int id, CancellationToken cancellationToken)
        {
            var user = await _repository.GetAsync(id, cancellationToken);
            if(user == null || user.UserDetail == null)
            {
                throw new UserNotFoundException($"User with the id {id} was not found");
            }

            return user.Adapt<AccountResponseModel>();
        }

        public async Task<AccountResponseModel> GetCompanyAsync(int id, CancellationToken cancellationToken)
        {
            var company = await _repository.GetAsync(id, cancellationToken);
            if (company == null || company.CompanyDetail == null)
            {
                throw new CompanyNotFoundException($"Company with the id {id} was not found");
            }

            return company.Adapt<AccountResponseModel>();
        }
    }
}
