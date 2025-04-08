using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Models.Response;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers.Events;
using offers.Application.Services.Transactions;
using offers.Application.UOF;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Offers
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryRepository _categoryRepository;

        private readonly IMediator _mediator;

        private readonly IUnitOfWork _unitOfWork;

        public OfferService(IOfferRepository offerRepository, IAccountRepository accountRepository, ICategoryRepository categoryRepository,IMediator mediator, IUnitOfWork unitOfWork)
        {
            _offerRepository = offerRepository;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }
        private async Task AccountIsActiveCheck(int accountId, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetAsync(accountId, cancellationToken); 
            if(account == null || account.CompanyDetail == null)
            {
                throw new CompanyNotFoundException($"company with the given account id: {accountId} was not found");
            }

            if (!account.CompanyDetail.IsActive)
            {
                throw new CompanyIsNotActiveException("you can't create or view your offer on a not activated account");
            }
        }
        public async Task<OfferResponseModel> CreateAsync(Offer offer, CancellationToken cancellationToken)
        {
            await PopulateOffer(offer, cancellationToken);
            await AccountIsActiveCheck(offer.AccountId, cancellationToken);

            await _offerRepository.CreateAsync(offer, cancellationToken);
            await _unitOfWork.SaveChangeAsync(cancellationToken);

            return offer.Adapt<OfferResponseModel>();
        }

        private async Task PopulateOffer(Offer offer, CancellationToken cancellationToken)
        {
            var offerAccount = await _accountRepository.GetAsync(offer.AccountId, cancellationToken);

            var offerCategory = await _categoryRepository.GetAsync(offer.CategoryId, cancellationToken);
            if (offerCategory == null)
            {
                throw new CategoryNotFoundException("this offer's category could not be found");
            }

            offer.Account = offerAccount;
            offer.Category = offerCategory;
        }
        public async Task<List<OfferResponseModel>> GetMyOffersAsync(int accountId, CancellationToken cancellationToken)
        {
            await AccountIsActiveCheck(accountId, cancellationToken);
            var offers = await _offerRepository.GetOffersByAccountIdAsync(accountId, cancellationToken);
          
            return offers.Adapt<List<OfferResponseModel>>() ?? new List<OfferResponseModel>();
        }
        public async Task<OfferResponseModel> GetMyOfferAsync(int id, int accountId,  CancellationToken cancellationToken)
        {
            await AccountIsActiveCheck(accountId, cancellationToken);
            var offer = await GetOfferAsync(id, cancellationToken);
            if(offer.AccountId != accountId)
            {
                throw new OfferAccessDeniedException($"You cannot access this offer because it does not belong to you");
            }

            return offer.Adapt<OfferResponseModel>();
        }

        public async Task DeleteAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            await AccountIsActiveCheck(accountId, cancellationToken);

            var offer = await GetOfferAsync(id, cancellationToken);
            ValidateDeleteOfferBusinessRules(id,accountId,offer);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _mediator.Publish(new OfferDeletedEvent(offer.Id), cancellationToken);

                _offerRepository.Delete(offer);

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
          
        private void ValidateDeleteOfferBusinessRules(int id, int accountId, Offer offer)
        {


            if (offer.AccountId != accountId)
            {
                throw new OfferAccessDeniedException("you can't delete an offer of another account");
            }

            if (DateTime.UtcNow > offer.CreatedAt + TimeSpan.FromMinutes(10))
            {
                throw new OfferCouldNotBeDeletedException("you can only delete an offer within 10 minutes of it's creation");
            }
        }

        public async Task<List<OfferResponseModel>> GetOffersByCategoriesAsync(List<int> categoryIds, CancellationToken cancellationToken)
        {

            var categories = await _categoryRepository.GetAllWithIdsAsync(categoryIds, cancellationToken);
            if(categories.Count < categoryIds.Count)
            {
                throw new CategoryNotFoundException("One or more categories that you provided were not found");
            }

            var offers = await _offerRepository.GetOffersByCategoriesAsync(categoryIds, cancellationToken);

            return offers?.Adapt<List<OfferResponseModel>>() ?? new List<OfferResponseModel>();
        }

        public async Task DecreaseStockAsync(int id, int count, CancellationToken cancellationToken)
        {
            var offer = await GetOfferAsync(id, cancellationToken);

            if (count > offer.Count)
            {
                throw new OfferCouldNotDecreaseStockException("could not decrease the stock of the offer due to request decrease amount exceeding the stock amount");
            }
;
            var StockCountBeforeDecrease = offer.Count;
            await _offerRepository.DecreaseStockAsync(id, count, cancellationToken);
            if (StockCountBeforeDecrease - offer.Count != count)
            {
                throw new OfferCouldNotDecreaseStockException("offer stock wasn't decreased by the expected amount");
            }

            await _unitOfWork.SaveChangeAsync(cancellationToken);
        }
        public async Task IncreaseStockAsync(int id, int count, CancellationToken cancellationToken)
        {
            var offer = await GetOfferAsync(id, cancellationToken);

            var BeforeIncreaseCount = offer.Count;
            await _offerRepository.IncreaseStockAsync(id, count, cancellationToken);
            if (offer.Count - BeforeIncreaseCount != count)
            {
                throw new OfferCouldNotIncreaseStockException("offer stock wasn't increased by the expected amount");
            }
            await _unitOfWork.SaveChangeAsync(cancellationToken);
        }

        [ExcludeFromCodeCoverage]
        public async Task ArchiveOffersAsync(CancellationToken cancellationToken)
        {
            await _offerRepository.ArchiveOffersAsync(cancellationToken);
        }

        [ExcludeFromCodeCoverage]
        public async Task<List<OfferResponseModel>> GetAllAsync(CancellationToken cancellationToken)
        {
            var categories = await _offerRepository.GetAllAsync(cancellationToken);


            return categories.Adapt<List<OfferResponseModel>>() ?? new List<OfferResponseModel>();
        }

        [ExcludeFromCodeCoverage]
        public async Task<OfferResponseModel> GetAsync(int id, CancellationToken cancellationToken)
        {
            var offer = await GetOfferAsync(id, cancellationToken);
            return offer.Adapt<OfferResponseModel>();
        }

        private async Task<Offer> GetOfferAsync(int id, CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetAsync(id, cancellationToken);
            if (offer == null)
            {
                throw new OfferNotFoundException($"offer with the id {id} was not found");

            }
            return offer;
        }
    }
}
