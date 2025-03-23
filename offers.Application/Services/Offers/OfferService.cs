using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Models;
using offers.Application.Services.Categories;
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
        private readonly ILogger<OfferService> _logger;

        public OfferService(IOfferRepository offerRepository, IAccountRepository accountRepository,ICategoryRepository categoryRepository, ILogger<OfferService> logger)
        {
            _offerRepository = offerRepository;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }
        public async Task CreateAsync(Offer offer, CancellationToken cancellationToken)
        {
            var offerAccount = await _accountRepository.GetAsync(offer.AccountId);

            if(offerAccount == null)
            {
                throw new AccountDoesNotExistException("this offer's account could not be found");
            }
            var offerCategory = await _categoryRepository.GetAsync(offer.CategoryId);
            if( offerCategory == null)
            {
                throw new 
            }

            offer.Account = offerAccount;
            offer.Category = offerCategory;

            var isAdded = await _offerRepository.CreateAsync(offer, cancellationToken);

            if (!isAdded)
            {
                _logger.LogError("Failed to create an offer {Name}, unknown issue", offer.Name);
                throw new OfferCouldNotBeCreatedException("Failed to create an offer because of an unknown issue");
            }
        }

        public async Task<List<OfferResponseModel>> GetMyOffersAsync(int accoundId, CancellationToken cancellationToken)
        {
            var offers = await _offerRepository.GetOffersByAccountIdAsync(accoundId, cancellationToken);

            return offers ?? new List<OfferResponseModel>();
        }
    }
}
