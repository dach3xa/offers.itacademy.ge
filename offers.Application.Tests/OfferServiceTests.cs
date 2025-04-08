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
using offers.Application.Services.Offers.Events;
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
                .ReturnsAsync(new Category
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
            Assert.Equal("you can't create or view your offer on a not activated account", exception.Message);
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

            var offerResponse = await _offerService.CreateAsync(offer, CancellationToken.None);

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

            var task = () => _offerService.GetMyOfferAsync(1, accountId, CancellationToken.None);

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
            Assert.Equal("you can't create or view your offer on a not activated account", exception.Message);
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

            _offerRepository
                .Setup(x => x.GetOffersByAccountIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>
                {
                    new Offer
                    {
                        Id = 1,
                        Name = "Super Saver Deal",
                        Description = "A limited-time offer for premium users.",
                        Count = 10,
                        Price = 49.99m,
                        IsArchived = false,
                        CategoryId = 1,
                        AccountId = 1,
                    }
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
            Assert.Equal("you can't create or view your offer on a not activated account", exception.Message);
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
            Assert.Equal("you can't create or view your offer on a not activated account", exception.Message);
        }

        [Fact(DisplayName = "when id does not exist delete should throw offer not found exception")]
        public async Task DeleteOffer_WhenIdDoesNotExist_ShouldThrowOfferNotFoundException()
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

            var task = () => _offerService.DeleteAsync(2, 1, CancellationToken.None);


            var exception = await Assert.ThrowsAsync<OfferNotFoundException>(task);
            Assert.Equal("offer with the id 2 was not found", exception.Message);
        }

        [Fact(DisplayName = "when offer isn't yours delete should throw offer access denied exception")]
        public async Task DeleteOffer_WhenOfferIsNotYours_ShouldThrowOfferAccessDeniedException()
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

            var task = () => _offerService.DeleteAsync(2, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferAccessDeniedException>(task);
            Assert.Equal("you can't delete an offer of another account", exception.Message);
        }

        [Fact(DisplayName = "when offer created at passes 10 minutes delete should throw offer could not be deleted exception")]
        public async Task DeleteOffer_WhenTenMinutesPassed_ShouldThrowOfferCouldNotBeDeletedException()
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
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                    });

            var task = () => _offerService.DeleteAsync(2, 1, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferCouldNotBeDeletedException>(task);
            Assert.Equal("you can only delete an offer within 10 minutes of it's creation", exception.Message);
        }

        [Fact(DisplayName = "when everything is checked correctly delete should delete the offer")]
        public async Task DeleteOffer_WhenEverythingPasses_ShouldDelete()
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
                        CreatedAt = DateTime.UtcNow,
                    });

            _mediator
                .Setup(m => m.Publish(It.IsAny<OfferDeletedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var task = () => _offerService.DeleteAsync(2, 1, CancellationToken.None);

            using (new AssertionScope())
            {
                task.Should().NotThrowAsync();
                _mediator.Verify(m => m.Publish(
                    It.Is<OfferDeletedEvent>(e => e.OfferId == 2),
                    It.IsAny<CancellationToken>()
                    ), Times.Once);

                _offerRepository.Verify(x => x.Delete(It.Is<Offer>(off => off.Id == 2)), Times.Once);
            }
        }

        [Theory(DisplayName = "when one or more category ids dont exist get offer by categories should throw category not found exception")]
        [InlineData(new int[] { 1, 2, 3 })]
        [InlineData(new int[] { 4, 5, 6 })]
        public async Task GetOffersByCategories_WhenIdDoesntExist_ShouldThrowCategoryNotFoundException(int[] idsParam)
        {
            var ids = idsParam.ToList();
            _categoryRepository
               .Setup(x => x.GetAllWithIdsAsync(ids, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<Category>());

            var task = () => _offerService.GetOffersByCategoriesAsync(ids, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CategoryNotFoundException>(task);
            Assert.Equal("One or more categories that you provided were not found", exception.Message);
        }

        [Theory(DisplayName = "when all ids exist get offer by categories should return the offers")]
        [InlineData(new int[] { 1, 2 })]
        [InlineData(new int[] { 4, 5 })]
        public async Task GetOffersByCategories_WhenIdsExist_ShouldReturnOffers(int[] idsParam)
        {
            var ids = idsParam.ToList();
            _categoryRepository
               .Setup(x => x.GetAllWithIdsAsync(ids, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<Category>
               {
                new Category
                {
                    Id = ids[0],
                    Name = "someCategory",
                    Description = "blablabla"
                },
                new Category
                {
                    Id = ids[1],
                    Name = "someCategory",
                    Description = "blablabla"
                }
               });

            _offerRepository
               .Setup(x => x.GetOffersByCategoriesAsync(ids, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<Offer>
               {
                new Offer
                    {
                        Id = 1,
                        Name = "Super Saver Deal",
                        Description = "A limited-time offer for premium users.",
                        Count = 10,
                        Price = 49.99m,
                        IsArchived = false,
                        CategoryId = ids[0],
                        AccountId = 1,
                        CreatedAt = DateTime.UtcNow,
                    },
                new Offer
                    {
                        Id = 2,
                        Name = "Super Saver Deal",
                        Description = "A limited-time offer for premium users.",
                        Count = 10,
                        Price = 49.99m,
                        IsArchived = false,
                        CategoryId = ids[1],
                        AccountId = 1,
                        CreatedAt = DateTime.UtcNow,
                    }
               });

            var offers = await _offerService.GetOffersByCategoriesAsync(ids, CancellationToken.None);

            using (new AssertionScope())
            {
                _offerRepository.Verify(x => x.GetOffersByCategoriesAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
                offers[0].CategoryId.Should().Be(ids[0]);
                offers[1].CategoryId.Should().Be(ids[1]);
            }
        }

        [Theory(DisplayName = "when id does not exist decrease stock should throw offer not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DecreaseStockOffer_WhenIdDoesNotExist_ShouldThrowOfferNotFoundException(int id)
        {
            _offerRepository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer)null);

            var task = () => _offerService.DecreaseStockAsync(id, 5, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferNotFoundException>(task);
            Assert.Equal($"offer with the id {id} was not found", exception.Message);
        }

        [Fact(DisplayName = "when request count is more than stock decrease stock should throw offer could not decrease stock exception")]
        public async Task DecreaseStockOffer_WhenRequestAmountIsMoreThanStock_ShouldThrowOfferCouldNotDecreaseStockException()
        {
            _offerRepository
            .Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
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
                CreatedAt = DateTime.UtcNow,
            });

            var task = () => _offerService.DecreaseStockAsync(1, 500, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferCouldNotDecreaseStockException>(task);
            Assert.Equal("could not decrease the stock of the offer due to request decrease amount exceeding the stock amount", exception.Message);
        }

        [Fact(DisplayName = "when validations are successful decrease stock should decrease the stock")]
        public async Task DecreaseStockOffer_WhenValidationSuccesful_ShouldDecreaseStock()
        {
            _offerRepository
                .Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Offer
                {
                    Id = 1,
                    Name = "Super Saver Deal",
                    Description = "A limited-time offer for premium users.",
                    Count = 10,
                    Price = 49.99m,
                    IsArchived = false,
                    CategoryId = 1,
                    AccountId = 1,
                    CreatedAt = DateTime.UtcNow,
                });

            try
            {
                await _offerService.DecreaseStockAsync(1, 5, CancellationToken.None);
            }
            catch
            {
                //ignore
            }

            using (new AssertionScope())
            {
                _offerRepository.Verify(x => x.DecreaseStockAsync(1, 5, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
        [Theory(DisplayName = "when id does not exist Increase stock should throw offer not found exception")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task IncreaseStockOffer_WhenIdDoesNotExist_ShouldThrowOfferNotFoundException(int id)
        {
            _offerRepository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer)null);

            var task = () => _offerService.IncreaseStockAsync(id, 5, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<OfferNotFoundException>(task);
            Assert.Equal($"offer with the id {id} was not found", exception.Message);
        }


        [Fact(DisplayName = "when validations are successful Increase stock should increase the stock")]
        public async Task IncreaseStockOffer_WhenValidationSuccesful_ShouldIncreaseStock()
        {
            _offerRepository
                .Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Offer
                {
                    Id = 1,
                    Name = "Super Saver Deal",
                    Description = "A limited-time offer for premium users.",
                    Count = 10,
                    Price = 49.99m,
                    IsArchived = false,
                    CategoryId = 1,
                    AccountId = 1,
                    CreatedAt = DateTime.UtcNow,
                });

            try
            {
                await _offerService.IncreaseStockAsync(1, 5, CancellationToken.None);
            }
            catch
            {
                //ignore
            }

            using (new AssertionScope())
            {
                _offerRepository.Verify(x => x.IncreaseStockAsync(1, 5, It.IsAny<CancellationToken>()), Times.Once);
            }
        }


    }
}
