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

namespace offers.Application.Services.Accounts
{
    public class AccountService : IAccountService
    {
        const string SECRET_KEY = "Secret_Hashing_Key";
        private readonly IAccountRepository _repository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository repository, ILogger<AccountService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<AccountResponseModel> LoginAsync(string Email, string password, CancellationToken cancellationToken)
        {
            var hashPassword = GenerateHash(password);
            var account = await _repository.GetAsync(Email, hashPassword, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Login failed for {Email}: Invalid credentials", Email);
                throw new AccountNotFoundException("Email or password is incorrect");
            }

            return account.Adapt<AccountResponseModel>();
        }

        public async Task RegisterAsync(Account account, CancellationToken cancellationToken)
        {
            var exists = await _repository.Exists(account.Email, cancellationToken);

            if (exists)
            {
                _logger.LogWarning("failed to register an account for {Email}: the email is already in use", account.Email);
                throw new AccountAlreadyExistsException("There already is an account with this email");
            }

            account.PasswordHash = GenerateHash(account.PasswordHash);
            var isRegistered = await _repository.RegisterAsync(account, cancellationToken);

            if (!isRegistered)
            {
                _logger.LogError("failed to register an account for {Email}: unknown issue", account.Email);
                throw new AccountCouldNotBeCreatedException("Failed to create account because of an unknown issue");
            }
        }

        public async Task<List<AccountResponseModel>> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            var companies = await _repository.GetAllCompaniesAsync(cancellationToken);

            return companies ?? new List<AccountResponseModel>();
        }

        public async Task<List<AccountResponseModel>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _repository.GetAllUsersAsync(cancellationToken);

            return users ?? new List<AccountResponseModel>();
        }

        public async Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken)
        {
            var companyAccount = await _repository.GetAsync(id, cancellationToken);

            if (companyAccount == null)
            {
                _logger.LogWarning("failed to Confirm a Company Account with the id {id} the company doesn't exist", id);
                throw new AccountNotFoundException("an account with the provided id does not exist");
            }

            if (companyAccount.CompanyDetail.IsActive)
            {
                _logger.LogWarning("Failed to Confirm a Company with the id {id} because its already active", id);
                throw new CompanyAlreadyActiveException("an account with the provided id is already active");
            }

            var isConfirmed = await _repository.ConfirmCompanyAsync(id, cancellationToken);

            if (!isConfirmed)
            {
                _logger.LogError("Failed to varify an account with id {id}: unknown issue", id);
                throw new AccountCouldNotBePatchedException("Failed to confirm an Account because of an unknown issue");
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

    }
}
