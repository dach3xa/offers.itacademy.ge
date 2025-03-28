﻿using Mapster;
using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Models;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Categories;
using offers.Application.Services.Transactions;
using offers.Application.UOF;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Offers
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryRepository _categoryRepository;

        private readonly ITransactionService _transactionService;

        private readonly IUnitOfWork _unitOfWork;

        public OfferService(IOfferRepository offerRepository, IAccountRepository accountRepository, ICategoryRepository categoryRepository, ITransactionService transactionService, IUnitOfWork unitOfWork)
        {
            _offerRepository = offerRepository;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _transactionService = transactionService;
            _unitOfWork = unitOfWork;
        }
        private async Task AccountIsActiveCheck(int accountId, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetAsync(accountId, cancellationToken); 
            if (!account.CompanyDetail.IsActive)
            {
                throw new CompanyIsNotActiveException("you can't create an offer on a not activated account ");
            }
        }
        public async Task<OfferResponseModel> CreateAsync(Offer offer, CancellationToken cancellationToken)
        {
            await PopulateOffer(offer, cancellationToken);
            await AccountIsActiveCheck(offer.AccountId, cancellationToken);

            var addedOffer = await _offerRepository.CreateAsync(offer, cancellationToken);

            if (addedOffer == null)
            {
                throw new OfferCouldNotBeCreatedException("Failed to create an offer because of an unknown issue");
            }

            return addedOffer.Adapt<OfferResponseModel>();
        }

        private async Task PopulateOffer(Offer offer, CancellationToken cancellationToken)
        {
            var offerAccount = await _accountRepository.GetAsync(offer.AccountId, cancellationToken);
            if (offerAccount == null)
            {
                throw new AccountNotFoundException("this offer's account could not be found");
            }

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
            var offer = await _offerRepository.GetAsync(id, cancellationToken);

            if(offer == null)
            {
                throw new OfferNotFoundException($"offer with the id {id} was not found");
            }
            if(offer.AccountId != accountId)
            {
                throw new OfferAccessDeniedException($"You cannot access this offer because it does not belong to you");
            }

            return offer.Adapt<OfferResponseModel>();
        }

        public async Task DeleteAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            await AccountIsActiveCheck(accountId, cancellationToken);

            var offer = await _offerRepository.GetAsync(id, cancellationToken);

            ValidateDeleteOfferBusinessRules(id,accountId,offer);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _transactionService.RefundAllUsersByOfferIdAsync(offer.Id, cancellationToken);

                var deletedOffer = await _offerRepository.DeleteAsync(offer, cancellationToken);

                if (deletedOffer == null)
                {
                    throw new OfferCouldNotBeDeletedException("Unknown Issue occured");
                }

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
            if (offer == null)
            {
                throw new OfferNotFoundException("Offer not found");
            }

            if (offer.AccountId != accountId)
            {
                throw new OfferAccessDeniedException("you can't delete an offer of another account");
            }

            if (DateTime.Now > offer.CreatedAt + TimeSpan.FromMinutes(10))
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
            var offer = await _offerRepository.GetAsync(id, cancellationToken);
            if(offer == null)
            {
                throw new OfferNotFoundException($"offer with the id: {id} could not be found");
            }

            if(count > offer.Count)
            {
              
                throw new OfferCouldNotDecreaseStockException("could not decrease the stock of the offer due to request decrease amount exceeding the stock amount");
            }

            var decreasedStockOffer = await _offerRepository.DecreaseStockAsync(id, count, cancellationToken);
            if(offer.Count - decreasedStockOffer.Count != count)
            {
                throw new OfferCouldNotDecreaseStockException("could not decrease the stock of the offer because of an unknown issue");
            }
        }
        public async Task IncreaseStockAsync(int id, int count, CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetAsync(id, cancellationToken);
            if (offer == null)
            {
                throw new OfferNotFoundException($"offer with the id: {id} could not be found");
            }

            var IncreasedStockOffer = await _offerRepository.IncreaseStockAsync(id, count, cancellationToken);
            if (IncreasedStockOffer.Count - offer.Count != count)
            {
                throw new OfferCouldNotIncreaseStockException("could not increase the stock of the offer because of an unknown issue");
            }
        }

        public async Task ArchiveOffersAsync(CancellationToken cancellationToken)
        {
            await _offerRepository.ArchiveOffersAsync(cancellationToken);
        }
    }
}
