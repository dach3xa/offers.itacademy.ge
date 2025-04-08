using offers.Application.Exceptions;
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
using System.Diagnostics.CodeAnalysis;
using offers.Application.Models.Response;
using Microsoft.AspNetCore.Identity;
using MediatR;
using System.Security.Claims;


namespace offers.Application.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IAccountRepository repository, UserManager<Account> userManager, SignInManager<Account> signInManager, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AccountResponseModel> LoginAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                throw new AccountNotFoundException("Email or password is incorrect");
            }

            return user.Adapt<AccountResponseModel>();
        }

        public async Task<AccountResponseModel> LoginMvcAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                throw new AccountNotFoundException("Email or password is incorrect");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return user.Adapt<AccountResponseModel>();
        }

        public async Task<AccountResponseModel> RegisterAsync(Account account, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(account.Email);
            if (user != null)
            {
                throw new AccountAlreadyExistsException("There already is an account with this email");
            }

            var result = await _userManager.CreateAsync(account, account.PasswordHash);//password is not hashed yet

            if (!result.Succeeded)
            {
                throw new AccountCouldNotBeCreatedException("Account could not be created because of an unknown error");
            }

            await _userManager.AddClaimsAsync(account,
                new List<Claim>() {
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim(ClaimTypes.Role, account.Role.ToString()),
                    new Claim("id", account.Id.ToString())
                });

            return account.Adapt<AccountResponseModel>();
        }

        [ExcludeFromCodeCoverage]
        public async Task<List<CompanyResponseModel>> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            var companies = await _repository.GetAllCompaniesAsync(cancellationToken);


            return companies.Adapt<List<CompanyResponseModel>>() ?? new List<CompanyResponseModel>();
        }

        [ExcludeFromCodeCoverage]
        public async Task<List<UserResponseModel>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _repository.GetAllUsersAsync(cancellationToken);

            return users.Adapt<List<UserResponseModel>>() ?? new List<UserResponseModel>();
        }

        public async Task ConfirmCompanyAsync(int id, CancellationToken cancellationToken)
        {
            var companyAccount = await _repository.GetAsync(id, cancellationToken);

            if (companyAccount == null || companyAccount.CompanyDetail == null)
            {
                throw new CompanyNotFoundException("a company with the provided id does not exist");
            }

            if (companyAccount.CompanyDetail.IsActive)
            {
                throw new CompanyAlreadyActiveException("an account with the provided id is already active");
            }

            await _repository.ConfirmCompanyAsync(id, cancellationToken);
            await _unitOfWork.SaveChangeAsync(cancellationToken);

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
                throw new UserNotFoundException("User with the provided id does not exist");
            }

            if (account.UserDetail.Balance < amount)
            {
                throw new InsufficientFundsException("this account doesn't have sufficient funds for the transaction");
            }

            decimal balanceBeforeWithdraw = account.UserDetail.Balance;
            await _repository.WithdrawAsync(accountId, amount, cancellationToken);

            if (balanceBeforeWithdraw - account.UserDetail.Balance != amount)
            {
                throw new AccountCouldNotWithdrawException($"Account with the id {accountId} could not withdraw the expected ammount because of an unknwon issue");
            }
            await _unitOfWork.SaveChangeAsync(cancellationToken);
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
                throw new UserNotFoundException("User with the provided id does not exist");
            }

            decimal balanceBeforeDeposit = account.UserDetail.Balance;
            await _repository.DepositAsync(accountId, amount, cancellationToken);
            if (account.UserDetail.Balance - balanceBeforeDeposit != amount)
            {
                throw new AccountCouldNotDepositException($"Deposit to account ID {accountId} failed the expected ammount could not be deposited because of an unknown issue");
            }
            await _unitOfWork.SaveChangeAsync(cancellationToken);
        }

        public async Task<UserResponseModel> GetUserAsync(int id, CancellationToken cancellationToken)
        {
            var user = await _repository.GetAsync(id, cancellationToken);
            if(user == null || user.UserDetail == null)
            {
                throw new UserNotFoundException($"User with the id {id} was not found");
            }

            return user.Adapt<UserResponseModel>();
        }

        public async Task<CompanyResponseModel> GetCompanyAsync(int id, CancellationToken cancellationToken)
        {
            var company = await _repository.GetAsync(id, cancellationToken);
            if (company == null || company.CompanyDetail == null)
            {
                throw new CompanyNotFoundException($"Company with the id {id} was not found");
            }

            return company.Adapt<CompanyResponseModel>();
        }
    }
}
