using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.API.Models;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Token;
using offers.Application.Exceptions.Transaction;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Application.Services.Transaction;
using offers.Domain.Enums;
using offers.Domain.Models;

namespace offers.API.Controllers
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.User))]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<UserController> _logger;
        private int _accountId;
        public UserController(ITransactionService transactionService,IOfferService offerService, ILogger<UserController> logger)
        {
            _transactionService = transactionService;
            _offerService = offerService;
            _logger = logger;
            _accountId = GetCurrentUserId();
        }
        private int GetCurrentUserId()
        {
            var accountIdString = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(accountIdString) || !int.TryParse(accountIdString, out var accountId))
            {
                _logger.LogError("Invalid or missing account Id");
                throw new InvalidTokenException("Invalid or missing account Id");
            }

            return accountId;
        }


        [HttpGet("offers")]
        public async Task<IActionResult> GetOffersByCategories([FromQuery] List<int> CategoryIds, CancellationToken cancellationToken)
        {
            var responseOffers = await _offerService.GetOffersByCategoriesAsync(CategoryIds, cancellationToken);

            return Ok(responseOffers);
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction(TransactionDTO transactionDTO, CancellationToken cancellationToken)
        {
            ValidateTransactionModelState();
            _logger.LogInformation("attempt to make a new Transaction on the offer with Id {OfferId}", transaction.OfferId);

            var transaction = transactionDTO.Adapt<Transaction>();
            transaction.AccountId = _accountId;
            await _transactionService.CreateAsync(transaction, cancellationToken);

            return StatusCode(201);
        }

        private void ValidateTransactionModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                throw new TransactionCouldNotValidateException("Could not validate the given transaction", errors);
            }
        }
    }
}
