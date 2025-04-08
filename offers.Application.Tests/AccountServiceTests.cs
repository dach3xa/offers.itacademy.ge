using Azure.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Moq;
using offers.Application.Exceptions;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions.Account.User;
using offers.Application.Exceptions.Funds;
using offers.Application.Models;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Accounts;
using offers.Application.UOF;
using offers.Domain.Enums;
using offers.Domain.Models;
using offers.Persistance.UOF;
using System;
using System.Data;
using System.Security.Principal;
using System.Threading.Tasks;

namespace offers.Application.Tests
{
    public class AccountServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IAccountRepository> _repository;

        private readonly Mock<UserManager<Account>> _userManager;
        private readonly Mock<SignInManager<Account>> _signInManager;

        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _repository = new Mock<IAccountRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();

            var store = new Mock<IUserStore<Account>>();
            _userManager = new Mock<UserManager<Account>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<Account>>();
            _signInManager = new Mock<SignInManager<Account>>(
                _userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null
            );

            _accountService = new AccountService(_repository.Object, _userManager.Object, _signInManager.Object, _unitOfWork.Object);
        }

        [Theory(DisplayName = "when an email exists return an account response model")]
        [InlineData("kirvalidzedachi@gmail.com")]
        [InlineData("chkheidzeguram@gmail.com")]
        public async Task LoginAccount_WhenEmailExists_ShouldReturnAccount(string email)
        {
            var hasher = new PasswordHasher<Account>();
            var account = new Account()
            {
                Id = 1,
                Email = email,
                PhoneNumber = "123-456-7890",
                Role = AccountRole.User,
                UserDetail = new UserDetail
                {
                    FirstName = "John",
                    LastName = "Doe",
                },
                CompanyDetail = null,
                Offers = new List<Offer>()

            };

            account.PasswordHash = hasher.HashPassword(account, "Dachidachi1.");
            _repository.Setup(x => x.GetAsync(It.Is<string>(s => s == email), It.IsAny<CancellationToken>())).
                ReturnsAsync(account);

            var accountResponse = await _accountService.LoginAsync(email, "Dachidachi1.", CancellationToken.None);

            using (new AssertionScope())
            {
                accountResponse.Should().NotBeNull();
                accountResponse.Email.Should().Be(email);
            }
        }

        [Theory(DisplayName = "when Password is incorrect throw account not found excpetion")]
        [InlineData("kirvalidzedachi@gmail.com")]
        [InlineData("chkheidzeguram@gmail.com")]
        public async Task LoginAccount_WhenPasswordIncorrect_ShouldThrowAccountNotFoundException(string email)
        {
            _repository.Setup(x => x.GetAsync(It.Is<string>(s => s == email), It.IsAny<CancellationToken>())).
                ReturnsAsync(new Account()
                {
                    Id = 1,
                    Email = email,
                    PhoneNumber = "123-456-7890",
                    PasswordHash = "wrong_password_hash",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()

                });

            var task = () => _accountService.LoginAsync(email, "123", CancellationToken.None);

            var exception = await Assert.ThrowsAsync<AccountNotFoundException>(task);
            Assert.Equal("Email or password is incorrect", exception.Message);
        }


        [Fact(DisplayName = "when email does not exist login should throw account not found exception")]
        public async Task LoginAccount_WhenEmailDoesNotExist_ShouldThrowAccountNotFoundException()
        {
            _repository
                .Setup(x => x.GetAsync("kirvalidzedachi@gmail.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task =  () => _accountService.LoginAsync("kirvalidzedachi@gmail.com","123",CancellationToken.None);

            var exception = await Assert.ThrowsAsync<AccountNotFoundException>(task);
            Assert.Equal("Email or password is incorrect", exception.Message);
        }

        [Fact(DisplayName = "when email exists register should throw account already exists exception")]
        public async Task RegisterAccount_WhenEmailExists_ShouldThrowAccountAlreadyExistsException()
        {
            var account = new Account
            {
                Id = 1,
                Email = "john@example.com",
                PasswordHash = "hashed123",
                Role = AccountRole.User,
                UserDetail = new UserDetail
                {
                    FirstName = "John",
                    LastName = "Doe"
                },
                Offers = new List<Offer>()
            };
            _userManager
                .Setup(x => x.FindByEmailAsync(account.Email))
                .ReturnsAsync(new Account
                {
                     Id = 1,
                });

            var task =  () => _accountService.RegisterAsync(account, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<AccountAlreadyExistsException>(task);
            Assert.Equal("There already is an account with this email", exception.Message);
        }

        [Fact(DisplayName = "when email does not exist register should return an account")]
        public async Task RegisterAccount_WhenEmailDoesNotExist_ShouldReturnAccount()
        {
            var account = new Account
            {
                Id = 1,
                Email = "john@example.com",
                PasswordHash = "hashed123",
                Role = AccountRole.User,
                UserDetail = new UserDetail
                {
                    FirstName = "John",
                    LastName = "Doe"
                },
                Offers = new List<Offer>()
            };

            _userManager
                .Setup(x => x.FindByEmailAsync(account.Email))
                .ReturnsAsync((Account)null);

            var accountResponse = await _accountService.RegisterAsync(account, CancellationToken.None);

            using (new AssertionScope())
            {
                accountResponse.Should().NotBeNull();
                accountResponse.Email.Should().Be(account.Email);
            }
        }

        [Theory(DisplayName = "when id does not exist confirm company should throw company not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task ConfirmCompanyAccount_WhenIdDoesNotExist_ShouldThrowAccountNotFoundException(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _accountService.ConfirmCompanyAsync(id, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal("a company with the provided id does not exist", exception.Message);
        }

        [Fact(DisplayName = "when id does not belong to a company, confirm company should throw company not found exception")]
        public async Task ConfirmCompanyAccount_WhenIdIsNotCompany_ShouldThrowAccountNotFoundException()
        {
            _repository
                .Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = 1,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            var task = () => _accountService.ConfirmCompanyAsync(1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal("a company with the provided id does not exist", exception.Message);
        }

        [Fact(DisplayName = "when company is already active, confirm company should throw company already active exception")]
        public async Task ConfirmCompanyAccount_WhenCompanyIsAlreadyActive_ShouldThrowCompanyAlreadyActiveException()
        {
            _repository
                .Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = 1,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.Company,
                    UserDetail = null,
                    CompanyDetail = new CompanyDetail
                    {
                        CompanyName = "SomeCompany",
                        IsActive = true,
                    },
                    Offers = new List<Offer>()
                });

            var task = () => _accountService.ConfirmCompanyAsync(1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyAlreadyActiveException>(task);
            Assert.Equal("an account with the provided id is already active", exception.Message);
        }

        [Theory(DisplayName = "when id exists and is company, confirm company should confirm the company")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task ConfirmCompanyAccount_WhenIdIsCompany_ShouldConfirmAccount(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = id,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.Company,
                    UserDetail = null,
                    CompanyDetail = new CompanyDetail
                    {
                        CompanyName = "SomeCompany",
                        IsActive = false,
                    },
                    Offers = new List<Offer>()
                });

            var task = () => _accountService.ConfirmCompanyAsync(id, CancellationToken.None);

            using (new AssertionScope())
            {
                task.Should().NotThrowAsync();
                _repository.Verify(x => x.ConfirmCompanyAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Theory(DisplayName = "when amount is less than zero deposit should throw an invalid operation exception")]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-3)]
        public async Task DepositAccount_WhenAmountIsLessThanZero_ShouldThrowInvalidOperationException(decimal amount)
        {
            var task = () => _accountService.DepositAsync(1, amount, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(task);
            Assert.Equal("Deposit amount must be greater than zero.", exception.Message);
        }

        [Theory(DisplayName = "when account id does not exist deposit should throw an account not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DepositAccount_WhenAccountIdDoesNotExist_ShouldThrowUserNotFoundException(int accountId)
        {
            _repository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _accountService.DepositAsync(accountId, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UserNotFoundException>(task);
            Assert.Equal("User with the provided id does not exist", exception.Message);

        }

        [Theory(DisplayName = "when account id is not a User Deposit should throw an account not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DepositAccount_WhenAccountIdIsNotUser_ShouldThrowUserNotFoundException(int accountId)
        {
            _repository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account {
                    Id = accountId,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.Company,
                    UserDetail = null,
                    CompanyDetail = new CompanyDetail
                    {
                        CompanyName = "SomeCompany",
                        IsActive = false,
                    },
                    Offers = new List<Offer>()
                });

            var task =  () => _accountService.DepositAsync(accountId, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UserNotFoundException>(task);
            Assert.Equal("User with the provided id does not exist", exception.Message);
        }


        [Theory(DisplayName = "when id exists and is a user than Deposit should Deposit")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DepositAccount_WhenAccountIdExistsAndIsUser_ShouldDeposit(int accountId)
        {
            _repository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = accountId,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Balance = 20
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            try
            {
                await _accountService.DepositAsync(accountId, 5, CancellationToken.None);
            }
            catch
            {
                // Ignore
            }

            using (new AssertionScope())
            {
                _repository.Verify(x => x.DepositAsync(accountId, 5, It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Theory(DisplayName = "when amount is less than zero withdraw should throw an invalid operation exception")]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-3)]
        public async Task WithdrawAccount_WhenAmountIsLessThanZero_ShouldThrowInvalidOperationException(decimal amount)
        {
            var task = () => _accountService.WithdrawAsync(1, amount, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(task);
            Assert.Equal("Withdrawal amount must be greater than zero.", exception.Message);
        }

        [Theory(DisplayName = "when account id does not exist withdraw should throw an account not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task WithdrawAccount_WhenAccountIdDoesNotExist_ShouldThrowUserNotFoundException(int accountId)
        {
            _repository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () =>  _accountService.WithdrawAsync(accountId, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UserNotFoundException>(task);
            Assert.Equal("User with the provided id does not exist", exception.Message);

        }

        [Theory(DisplayName = "when account id is not a User withdraw should throw an account not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task WithdrawAccount_WhenAccountIdIsNotUser_ShouldThrowUserNotFoundException(int accountId)
        {
            _repository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = accountId,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.Company,
                    UserDetail = null,
                    CompanyDetail = new CompanyDetail
                    {
                        CompanyName = "SomeCompany",
                        IsActive = false,
                    },
                    Offers = new List<Offer>()
                });

            var task =  () =>  _accountService.WithdrawAsync(accountId, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UserNotFoundException>(task);
            Assert.Equal("User with the provided id does not exist", exception.Message);
        }

        [Theory(DisplayName = "when user balance is less than given amount withdraw should throw an insufficient funds exception")]
        [InlineData(6)]
        [InlineData(16)]
        [InlineData(26)]
        public async Task WithdrawAccount_WhenUserBalanceIsLessThanAmount_ShouldThrowInsufficientFundsException(decimal amount)
        {
            _repository
                .Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = 1,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Balance = 5
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            var task =  () =>  _accountService.WithdrawAsync(1, amount, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<InsufficientFundsException>(task);
            Assert.Equal("this account doesn't have sufficient funds for the transaction", exception.Message);
        }


        [Theory(DisplayName = "when id exists and is a user than withdraw should withdraw")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task WithdrawAccount_WhenAccountIdExistsAndIsUser_ShouldWithdraw(int accountId)
        {
            _repository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = accountId,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Balance = 20
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            try
            {
                await _accountService.WithdrawAsync(accountId, 5, CancellationToken.None);
            }
            catch
            {
                //ignore
            }

            using (new AssertionScope())
            {
                _repository.Verify(x => x.WithdrawAsync(accountId, 5, It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Theory(DisplayName = "when id does not exist get user should throw user not found excpetion")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetUserAccount_WhenIdDoesNotExist_ShouldThrowUserNotFoundException(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _accountService.GetUserAsync(id, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UserNotFoundException>(task);
            Assert.Equal($"User with the id {id} was not found", exception.Message);
        }

        [Theory(DisplayName = "when id is not user, get user should throw user not found excpetion")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetUserAccount_WhenIdIsNotUser_ShouldThrowUserNotFoundException(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _accountService.GetUserAsync(id, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UserNotFoundException>(task);
            Assert.Equal($"User with the id {id} was not found", exception.Message);
        }

        [Theory(DisplayName = "when id exists and is user, get user should get the user")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetUserAccount_WhenIdExistsAndIsUser_ShouldReturnUser(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = id,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Balance = 20
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            var user = await _accountService.GetUserAsync(id, CancellationToken.None);

            using (new AssertionScope())
            {
                user.Should().NotBeNull();
                user.Id.Should().Be(id);
            }
        }

        [Theory(DisplayName = "when id does not exist get Company should throw Company not found excpetion")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetCompanyAccount_WhenIdDoesNotExist_ShouldThrowCompanyNotFoundException(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _accountService.GetCompanyAsync(id, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"Company with the id {id} was not found", exception.Message);
        }

        [Theory(DisplayName = "when id is not Company, get company should throw company not found excpetion")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetCompanyAccount_WhenIdIsNotCompany_ShouldThrowCompanyNotFoundException(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _accountService.GetCompanyAsync(id, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"Company with the id {id} was not found", exception.Message);
        }

        [Theory(DisplayName = "when id exists and is Company, get Company should get the Company")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetCompanyAccount_WhenIdExistsAndIsCompany_ShouldReturnCompany(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = id,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.Company,
                    UserDetail = null,
                    CompanyDetail = new CompanyDetail
                    {
                        CompanyName = "somecompany"
                    },
                    Offers = new List<Offer>()
                });

            var user = await _accountService.GetCompanyAsync(id, CancellationToken.None);

            using (new AssertionScope())
            {
                user.Should().NotBeNull();
                user.Id.Should().Be(id);
            }
        }
    }
}