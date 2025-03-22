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

namespace offers.Application.Accounts
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

            if(account == null)
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
            var IsRegistered =  await _repository.RegisterAsync(account, cancellationToken);

            if (!IsRegistered)
            {
                _logger.LogError("failed to register an account for {Email}: unknown issue", account.Email);
                throw new AccountCouldNotBeCreatedException("Failed to create account.");
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
