using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Models;
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

        private readonly ILogger<OfferService> _logger;

        public OfferService(IOfferRepository offerRepository, IAccountRepository accountRepository, ICategoryRepository categoryRepository, ITransactionService transactionService, IUnitOfWork unitOfWork, ILogger<OfferService> logger)
        {
            _offerRepository = offerRepository;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _transactionService = transactionService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task CreateAsync(Offer offer, CancellationToken cancellationToken)
        {

            await PopulateOffer(offer, cancellationToken);
            var isAdded = await _offerRepository.CreateAsync(offer, cancellationToken);

            if (!isAdded)
            {
                _logger.LogError("Failed to create an offer {Name}, unknown issue", offer.Name);
                throw new OfferCouldNotBeCreatedException("Failed to create an offer because of an unknown issue");
            }
        }

        private async Task PopulateOffer(Offer offer, CancellationToken cancellationToken)
        {
            var offerAccount = await _accountRepository.GetAsync(offer.AccountId, cancellationToken);
            if (offerAccount == null)
            {
                _logger.LogError("Failed to create an offer {Name}, offer's Account could not be found", offer.Name);
                throw new AccountNotFoundException("this offer's account could not be found");
            }

            var offerCategory = await _categoryRepository.GetAsync(offer.CategoryId, cancellationToken);
            if (offerCategory == null)
            {
                _logger.LogError("Failed to create an offer {Name}, offer's category could not be found", offer.Name);
                throw new CategoryNotFoundException("this offer's category could not be found");
            }

            offer.Account = offerAccount;
            offer.Category = offerCategory;
        }
        public async Task<List<OfferResponseModel>> GetMyOffersAsync(int accoundId, CancellationToken cancellationToken)
        {
            var offers = await _offerRepository.GetOffersByAccountIdAsync(accoundId, cancellationToken);
          
            return offers ?? new List<OfferResponseModel>();
        }

        public async Task DeleteAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetAsync(id);

            ValidateDeleteOfferBusinessRules(id,accountId,offer);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _transactionService.RefundAllByOfferIdAsync(offer.Id, cancellationToken);

                var IsDeleted = await _offerRepository.DeleteAsync(offer, cancellationToken);

                if (!IsDeleted)
                {
                    _logger.LogWarning("Failed to delete offer with ID {id} due to an unknown issue.", id);
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
                _logger.LogWarning("Attempt to delete offer with ID {id} denied: id was not found", id);
                throw new OfferNotFoundException("Offer not found");
            }

            if (offer.AccountId != accountId)
            {
                _logger.LogWarning("Attempt to delete offer with ID {id} was denied due to account mismatch", id);
                throw new OfferAccessDeniedException("you can't delete an offer of another account");
            }

            if (DateTime.Now > offer.CreatedAt + TimeSpan.FromMinutes(10))
            {
                _logger.LogWarning("Attempt to delete offer with ID {id} was denied due to 10 minute timer passing", id);
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


            var offers = await _categoryRepository.GetOffersByCategoriesAsync(categoryIds, cancellationToken);

            return offers?.Adapt<List<OfferResponseModel>>() ?? new List<OfferResponseModel>();
        }
    }
}
