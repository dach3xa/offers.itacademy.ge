using FluentAssertions;
using FluentAssertions.Execution;
using MediatR;
using Microsoft.Identity.Client;
using Moq;
using offers.Application.Exceptions;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Application.UOF;
using offers.Domain.Enums;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Tests
{
    public class OfferServiceTests
    {
        private readonly Mock<IOfferRepository> _offerRepository;
        private readonly Mock<IAccountRepository> _accountRepository;
        private readonly Mock<ICategoryRepository> _categoryRepository;

        private readonly Mock<IUnitOfWork> _unitOfWork;

        private readonly Mock<IMediator> _mediator;

        private readonly OfferService _offerService;

        public OfferServiceTests()
        {
            _offerRepository = new Mock<IOfferRepository>();
            _accountRepository = new Mock<IAccountRepository>();
            _categoryRepository = new Mock<ICategoryRepository>();
            _mediator = new Mock<IMediator>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _offerService = new OfferService(_offerRepository.Object, _accountRepository.Object, _categoryRepository.Object, _mediator.Object, _unitOfWork.Object);
        }

        [Fact(DisplayName = "when account id does not exist create offer should throw account not found exception")]
        public async Task CreateOffer_WhenAccountIdDoesNotExist_ShouldThrowCompanyNotFoundException()
        {
            var offer = new Offer
            {
                Id = 1,
                Name = "Super Saver Deal",
                Description = "A limited-time offer for premium users.",
                Count = 10,
                Price = 49.99m,
                IsArchived = false,
                CategoryId = 1,
                AccountId = 1,
            };

            _categoryRepository
                .Setup(x => x.GetAsync(offer.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync( new Category
                {
                    Id = offer.CategoryId,
                    Name = "Electronics",
                    Description = "Gadgets, devices, and tech accessories",
                });

            _accountRepository
                .Setup(x => x.GetAsync(offer.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _offerService.CreateAsync(offer, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {offer.AccountId} was not found", exception.Message);
        }

        [Fact(DisplayName = "when account id is not company create offer should throw account not found exception")]
        public async Task CreateOffer_WhenAccountIdIsNotCompany_ShouldThrowCompanyNotFoundException()
        {
            var offer = new Offer
            {
                Id = 1,
                Name = "Super Saver Deal",
                Description = "A limited-time offer for premium users.",
                Count = 10,
                Price = 49.99m,
                IsArchived = false,
                CategoryId = 1,
                AccountId = 1,
            };

            _categoryRepository
                .Setup(x => x.GetAsync(offer.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category
                {
                    Id = offer.CategoryId,
                    Name = "Electronics",
                    Description = "Gadgets, devices, and tech accessories",
                });

            _accountRepository
                .Setup(x => x.GetAsync(offer.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = offer.AccountId,
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

            var task = () => _offerService.CreateAsync(offer, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {offer.AccountId} was not found", exception.Message);
        }

        [Fact(DisplayName = "when company is not active create offer should throw company is not active exception")]
        public async Task CreateOffer_WhenCompanyIsNotActive_ShouldThrowCompanyIsNotActiveException()
        {
            var offer = new Offer
            {
                Id = 1,
                Name = "Super Saver Deal",
                Description = "A limited-time offer for premium users.",
                Count = 10,
                Price = 49.99m,
                IsArchived = false,
                CategoryId = 1,
                AccountId = 1,
            };

            _categoryRepository
                .Setup(x => x.GetAsync(offer.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category
                {
                    Id = offer.CategoryId,
                    Name = "Electronics",
                    Description = "Gadgets, devices, and tech accessories",
                });

            _accountRepository
                .Setup(x => x.GetAsync(offer.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = offer.AccountId,
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

            var task = () => _offerService.CreateAsync(offer, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyIsNotActiveException>(task);
            Assert.Equal("you can't create an offer on a not activated account", exception.Message);
        }


        [Fact(DisplayName = "when category id does not exist create offer should throw category not found exception")]
        public async Task CreateOffer_WhenCategoryIdDoesNotExist_ShouldThrowCategoryNotFoundException()
        {
            var offer = new Offer
            {
                Id = 1,
                Name = "Super Saver Deal",
                Description = "A limited-time offer for premium users.",
                Count = 10,
                Price = 49.99m,
                IsArchived = false,
                CategoryId = 1,
                AccountId = 1,
            };
            _categoryRepository
                .Setup(x => x.GetAsync(offer.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category)null);

            var task = () => _offerService.CreateAsync(offer, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CategoryNotFoundException>(task);
            Assert.Equal("this offer's category could not be found", exception.Message);
        }

        [Fact(DisplayName = "when company is active and offer populates correctly create should return the offer")]
        public async Task CreateOffer_WhenCompanyExistsAndIsActive_ShouldReturnOffer()
        {
            var offer = new Offer
            {
                Id = 1,
                Name = "Super Saver Deal",
                Description = "A limited-time offer for premium users.",
                Count = 10,
                Price = 49.99m,
                IsArchived = false,
                CategoryId = 1,
                AccountId = 1,
            };

            _categoryRepository
                .Setup(x => x.GetAsync(offer.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category
                {
                    Id = offer.CategoryId,
                    Name = "Electronics",
                    Description = "Gadgets, devices, and tech accessories",
                });

            _accountRepository
                .Setup(x => x.GetAsync(offer.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Account
                {
                    Id = offer.AccountId,
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

            var offerResponse =  await _offerService.CreateAsync(offer, CancellationToken.None);

            using (new AssertionScope())
            {
                offerResponse.Should().NotBeNull();
                offerResponse.Id.Should().Be(offer.Id);
            }
        }

        [Theory(DisplayName = "when account id does not exist get my offer should throw company not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyOffer_WhenAccountIdDoesNotExist_ShouldThrowCompanyNotFoundException(int accountId)
        {
            _accountRepository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _offerService.GetMyOfferAsync(accountId, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {accountId} was not found", exception.Message);
        }

        [Theory(DisplayName = "when account with the id is not company get my offer should throw company not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyOffer_WhenAccountIdIsNotCompany_ShouldThrowCompanyNotFoundException(int accountId)
        {
            _accountRepository
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

            var task = () => _offerService.GetMyOfferAsync(1, accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {accountId} was not found", exception.Message);
        }

        [Theory(DisplayName = "when company with the id is not active get my offer should throw company Is Not Active Exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyOffer_WhenCompanyIsNotActive_ShouldThrowCompanyIsNotActiveException(int accountId)
        {
            _accountRepository
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

            var task = () => _offerService.GetMyOfferAsync(1, accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyIsNotActiveException>(task);
            Assert.Equal("you can't create an offer on a not activated account", exception.Message);
        }

        [Fact(DisplayName = "when offer id does not exist get my offer should throw offer not found exception")]
        public async Task GetMyOffer_WhenOfferIdDoesNotExist_ShouldThrowOfferNotFoundException()
        {
            _accountRepository
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

            _offerRepository
                .Setup(x => x.GetAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer)null);

            var task = () => _offerService.GetMyOfferAsync(2, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferNotFoundException>(task);
            Assert.Equal($"offer with the id {2} was not found", exception.Message);
        }

        [Fact(DisplayName = "when offer does not belong to the given company get my offer should throw offer access denied exception")]
        public async Task GetMyOffer_WhenOfferDoesNotBelongToCompany_ShouldThrowOfferAccessDeniedException()
        {
            _accountRepository
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

            _offerRepository
                .Setup(x => x.GetAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Offer
                {
                    Id = 2,
                    Name = "Super Saver Deal",
                    Description = "A limited-time offer for premium users.",
                    Count = 10,
                    Price = 49.99m,
                    IsArchived = false,
                    CategoryId = 1,
                    AccountId = 3,
                });

            var task = () => _offerService.GetMyOfferAsync(2, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferAccessDeniedException>(task);
            Assert.Equal($"You cannot access this offer because it does not belong to you", exception.Message);
        }

        [Fact(DisplayName = "when offer does belong to the given company get my offer return the offer")]
        public async Task GetMyOffer_WhenOfferDoesBelongToCompany_ShouldReturnOffer()
        {
            _accountRepository
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

            _offerRepository
                .Setup(x => x.GetAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Offer
                {
                    Id = 2,
                    Name = "Super Saver Deal",
                    Description = "A limited-time offer for premium users.",
                    Count = 10,
                    Price = 49.99m,
                    IsArchived = false,
                    CategoryId = 1,
                    AccountId = 1,
                });

            var offerResponse = await _offerService.GetMyOfferAsync(2, 1, CancellationToken.None);

            using (new AssertionScope())
            {
                offerResponse.Should().NotBeNull();
                offerResponse.Id.Should().Be(2);
            }
        }
        //------
        [Fact(DisplayName = "when company with the id is active and exists should return offers")]
        public async Task GetMyOffers_WhenCompanyIsActiveAndExists_ShouldReturnOffers()
        {
            _accountRepository
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

            var offerResponses = await _offerService.GetMyOffersAsync(1, CancellationToken.None);

            using (new AssertionScope())
            {
                offerResponses.Should().NotBeNull();
                offerResponses[0].Id.Should().Be(1);
            }
        }

        [Theory(DisplayName = "when account id does not exist get my offers should throw company not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyOffers_WhenAccountIdDoesNotExist_ShouldThrowCompanyNotFoundException(int accountId)
        {
            _accountRepository
                .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Account)null);

            var task = () => _offerService.GetMyOffersAsync(accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {accountId} was not found", exception.Message);
        }

        [Theory(DisplayName = "when account with the id is not company get my offers should throw company not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyOffers_WhenAccountIdIsNotCompany_ShouldThrowCompanyNotFoundException(int accountId)
        {
            _accountRepository
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

            var task = () => _offerService.GetMyOffersAsync(accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {accountId} was not found", exception.Message);
        }

        [Theory(DisplayName = "when company with the id is not active get my offers should throw company Is Not Active Exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetMyOffers_WhenCompanyIsNotActive_ShouldThrowCompanyIsNotActiveException(int accountId)
        {
            _accountRepository
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

            var task = () => _offerService.GetMyOffersAsync(accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyIsNotActiveException>(task);
            Assert.Equal("you can't create an offer on a not activated account", exception.Message);
        }

        [Theory(DisplayName = "when account id does not exist Delete offer should throw company not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DeleteOffer_WhenCompanyIdDoesNotExist_ShouldThrowCompanyNotFoundException(int accountId)
        {
            _accountRepository
            .Setup(x => x.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account)null);

            var task = () => _offerService.DeleteAsync(1, accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {accountId} was not found", exception.Message);
        }

        [Theory(DisplayName = "when account with the id is not company Delete Offer should throw company not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DeleteOffer_WhenAccountIdIsNotCompany_ShouldThrowCompanyNotFoundException(int accountId)
        {
            _accountRepository
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

            var task = () => _offerService.DeleteAsync(1, accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(task);
            Assert.Equal($"company with the given account id: {accountId} was not found", exception.Message);
        }

        [Theory(DisplayName = "when company with the id is not active delete offers should throw company Is Not Active Exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DeleteOffer_WhenCompanyIsNotActive_ShouldThrowCompanyIsNotActiveException(int accountId)
        {
            _accountRepository
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

            var task = () => _offerService.DeleteAsync(1, accountId, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CompanyIsNotActiveException>(task);
            Assert.Equal("you can't create an offer on a not activated account", exception.Message);
        }
    }
}
