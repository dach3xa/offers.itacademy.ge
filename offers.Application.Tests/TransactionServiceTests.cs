using MediatR;
using Moq;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using offers.Application.Services.Transactions;
using offers.Application.UOF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using offers.Domain.Models;
using Microsoft.Identity.Client;
using offers.Application.Exceptions.Account.User;
using offers.Domain.Enums;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Transaction;
using FluentAssertions.Execution;
using FluentAssertions;

namespace offers.Application.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<IOfferRepository> _offerRepository;
        private readonly Mock<IAccountRepository> _accountRepository;
        private readonly Mock<ITransactionRepository> _transactionRepository;

        private readonly Mock<IOfferService> _offerService;
        private readonly Mock<IAccountService> _accountService;

        private readonly Mock<IUnitOfWork> _unitOfWork;

        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _offerRepository = new Mock<IOfferRepository>();
            _accountRepository = new Mock<IAccountRepository>();
            _transactionRepository = new Mock<ITransactionRepository>();

            _offerService = new Mock<IOfferService>();
            _accountService = new Mock<IAccountService>();

            _unitOfWork = new Mock<IUnitOfWork>();


            _transactionService = new TransactionService(_transactionRepository.Object, _accountRepository.Object, _offerRepository.Object, _offerService.Object, _accountService.Object, _unitOfWork.Object);
        }

        [Fact(DisplayName = "when transactions user id does not exist create should throw account not found exception")]
        public async Task CreateTransaction_WhenTransactionUserIdDoesNotExist_ShouldThrowAccountNotFoundException()
        {
            var transaction = new Transaction
            {
                Id = 1,
                Count = 3,
                Paid = 149.97m,
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                OfferId = 3
            };

            _accountRepository
                .Setup(x => x.GetAsync(transaction.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _transactionService.CreateAsync(transaction, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UserNotFoundException>(task);
            Assert.Equal("this transaction's user account could not be found", exception.Message);
        }

        [Fact(DisplayName = "when transactions offer id does not exist create should throw offer not found exception")]
        public async Task CreateTransaction_WhenTransactionOfferIdDoesNotExist_ShouldThrowOfferNotFoundException()
        {
            var transaction = new Transaction
            {
                Id = 1,
                Count = 3,
                Paid = 149.97m,
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                OfferId = 3
            };

            _accountRepository
                .Setup(x => x.GetAsync(transaction.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = transaction.UserId,
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

            _offerRepository.Setup(x => x.GetAsync(transaction.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer)null);

            var task = () => _transactionService.CreateAsync(transaction, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferNotFoundException>(task);
            Assert.Equal("this transaction's offer could not be found", exception.Message);
        }

        [Fact(DisplayName = "when transactions paid amount is invalid create should throw offer not found exception")]
        public async Task CreateTransaction_WhenTransactionPaidIsInvalid_ShouldThrowTransactionCouldNotBeCreatedException()
        {
            var transaction = new Transaction
            {
                Id = 1,
                Count = 3,
                Paid = 150m,
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                OfferId = 3
            };

            _accountRepository
                .Setup(x => x.GetAsync(transaction.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = transaction.UserId,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Balance = 300
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            _offerRepository.Setup(x => x.GetAsync(transaction.OfferId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Offer
                {
                    Id = transaction.OfferId,
                    Name = "Super Saver Deal",
                    Description = "A limited-time offer for premium users.",
                    Count = 10,
                    Price = 10m,
                    IsArchived = false,
                    CategoryId = 1,
                    AccountId = 1,
                });

            var task = () => _transactionService.CreateAsync(transaction, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<TransactionCouldNotBeCreatedException>(task);
            Assert.Equal("Paid amount does not match the expected total", exception.Message);
        }

        [Fact(DisplayName = "when transactions offer is archived create should throw offer expired exceotion")]
        public async Task CreateTransaction_WhenOfferIsArchived_ShouldThrowOfferExpiredException()
        {
            var transaction = new Transaction
            {
                Id = 1,
                Count = 3,
                Paid = 150m,
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                OfferId = 3
            };

            _accountRepository
                .Setup(x => x.GetAsync(transaction.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = transaction.UserId,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Balance = 300m
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            _offerRepository.Setup(x => x.GetAsync(transaction.OfferId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Offer
                {
                    Id = transaction.OfferId,
                    Name = "Super Saver Deal",
                    Description = "A limited-time offer for premium users.",
                    Count = 10,
                    Price = 50m,
                    IsArchived = true,
                    CategoryId = 1,
                    AccountId = 1,
                });

            var task = () => _transactionService.CreateAsync(transaction, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferExpiredException>(task);
            Assert.Equal("the offer that you are trying to access is archived", exception.Message);
        }

        [Fact(DisplayName = "when validation is succesfull create transaction should create the transaction")]
        public async Task CreateTransaction_WhenValidationSuccesfull_ShouldCreate()
        {
            var transaction = new Transaction
            {
                Id = 1,
                Count = 3,
                Paid = 150m,
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                OfferId = 3
            };

            _accountRepository
                .Setup(x => x.GetAsync(transaction.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = transaction.UserId,
                    Email = "john@example.com",
                    PasswordHash = "hashed123",
                    Role = AccountRole.User,
                    UserDetail = new UserDetail
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Balance = 300m
                    },
                    CompanyDetail = null,
                    Offers = new List<Offer>()
                });

            _offerRepository.Setup(x => x.GetAsync(transaction.OfferId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Offer
                {
                    Id = transaction.OfferId,
                    Name = "Super Saver Deal",
                    Description = "A limited-time offer for premium users.",
                    Count = 10,
                    Price = 50m,
                    IsArchived = false,
                    CategoryId = 1,
                    AccountId = 1,
                });

            var transactionResponse = await _transactionService.CreateAsync(transaction, CancellationToken.None);

            using (new AssertionScope())
            {
                _accountService.Verify(x => x.WithdrawAsync(transaction.UserId,transaction.Paid, It.IsAny<CancellationToken>()), Times.Once);
                _offerService.Verify(x => x.DecreaseStockAsync(transaction.OfferId, transaction.Count, It.IsAny<CancellationToken>()), Times.Once);
                _transactionRepository.Verify(x => x.CreateAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
                transactionResponse.Id.Should().Be(transaction.Id);
            }
        }

        [Theory(DisplayName = "when id does not exist get my transaction should throw transaction not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyTransaction_WhenIdDoesNotExist_ShouldThrowTransactionNotFoundException(int id)
        {
            _transactionRepository.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Transaction)null);

            var task = () => _transactionService.GetMyTransactionAsync(id, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<TransactionNotFoundException>(task);
            Assert.Equal($"Transaction with the id {id} was not found", exception.Message);
        }

        [Theory(DisplayName = "when account id is invalid get my transaction should throw transaction access denied exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyTransaction_WhenInvalidAccountId_ShouldThrowTransactionAccessDeniedException(int id)
        {
            _transactionRepository.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Transaction
                    {
                        Id = id,
                        Count = 3,
                        Paid = 150m,
                        CreatedAt = DateTime.UtcNow,
                        UserId = 2,
                        OfferId = 3
                    });

            var task = () => _transactionService.GetMyTransactionAsync(id, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<TransactionAccessDeniedException>(task);
            Assert.Equal("You cannot access this Transaction because it does not belong to you", exception.Message);
        }

        [Fact(DisplayName = "when validations are succesful get my transaction should return the transaction")]
        public async Task GetMyTransaction_WhenValidationSuccessful_ShouldReturnTransaction()
        {
            _transactionRepository.Setup(x => x.GetAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 2,
                    Count = 3,
                    Paid = 150m,
                    CreatedAt = DateTime.UtcNow,
                    UserId = 1,
                    OfferId = 3
                });

            var transaction = await _transactionService.GetMyTransactionAsync(2, 1, CancellationToken.None);

            using (new AssertionScope())
            {
                transaction.Id.Should().Be(2);
            }
        }

        [Fact(DisplayName = "when id does not exist refund should throw transaction not found exception")]
        public async Task RefundTransaction_IdDoesNotExist_ShouldThrowTransactionNotFoundException()
        {
            _transactionRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Transaction)null);

            var task = () => _transactionService.RefundAsync(1, 2, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<TransactionNotFoundException>(task);
            Assert.Equal($"Transaction with the id {1} was not found", exception.Message);
        }

        [Fact(DisplayName = "when account id is invalid refund should throw transaction access denied exception")]
        public async Task RefundTransaction_WhenInvalidAccountId_ShouldThrowTransactionAccessDeniedException()
        {
            _transactionRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Transaction
                    {
                        Id = 1,
                        Count = 3,
                        Paid = 150m,
                        CreatedAt = DateTime.UtcNow,
                        UserId = 4,
                        OfferId = 3
                    });

            var task = () => _transactionService.RefundAsync(1, 2, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<TransactionAccessDeniedException>(task);
            Assert.Equal("You cannot access this Transaction because it does not belong to you", exception.Message);
        }

        [Fact(DisplayName = "When 5 minutes passes from the creation of the transaction refund should throw transaction could not be refunded exception")]
        public async Task RefundTransaction_When5minutesPasses_ShouldThrowTransactionCouldNotBeRefundedException()
        {
            _transactionRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    Count = 3,
                    Paid = 150m,
                    CreatedAt = DateTime.UtcNow.AddDays(1),
                    UserId = 2,
                    OfferId = 3
                });

            var task = () => _transactionService.RefundAsync(1, 2, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<TransactionCouldNotBeRefundedException>(task);
            Assert.Equal("you can only refund a transaction within 5 minutes", exception.Message);
        }

        [Fact(DisplayName = "when validation is succesful refund the transaction")]
        public async Task RefundTransaction_WhenValidationSuccesful_ShouldRefundTransaction()
        {
            _transactionRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Transaction
                {
                    Id = 1,
                    Count = 3,
                    Paid = 150m,
                    CreatedAt = DateTime.UtcNow,
                    UserId = 2,
                    OfferId = 3
                });


            var task = () => _transactionService.RefundAsync(1, 2, CancellationToken.None);

            using (new AssertionScope())
            {
                task.Should().NotThrowAsync();
                _offerService.Verify(x => x.IncreaseStockAsync(3, 3, It.IsAny<CancellationToken>()), Times.Once);
                _accountService.Verify(x => x.DepositAsync(2, 150m, It.IsAny<CancellationToken>()), Times.Once);
                _transactionRepository.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
