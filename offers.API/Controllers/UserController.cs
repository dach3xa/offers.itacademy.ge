using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.API.Models;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Token;
using offers.Application.Exceptions.Transaction;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.API.Controllers.Helper;
using offers.Domain.Enums;
using offers.Domain.Models;
using offers.Application.Exceptions.Deposit;
using offers.Application.Services.Transactions;
using Mapster;

namespace offers.API.Controllers
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.User))]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;

        private readonly ILogger<UserController> _logger;

        private int _accountId;
        public UserController(ITransactionService transactionService, IOfferService offerService, ILogger<UserController> logger)
        {
            _transactionService = transactionService;
            _offerService = offerService;
            _logger = logger;
            _accountId = ControllerHelper.GetUserIdFromClaims(User);
        }


        [HttpGet("offers")]
        public async Task<IActionResult> GetOffersByCategories([FromQuery] List<int> categoryIds, CancellationToken cancellationToken)
        {
            var responseOffers = await _offerService.GetOffersByCategoriesAsync(categoryIds, cancellationToken);

            return Ok(responseOffers);
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction(TransactionDTO transactionDTO, CancellationToken cancellationToken)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => new TransactionCouldNotValidateException("Validation failed", errors));

            _logger.LogInformation("attempt to make a new Transaction on the offer with Id {OfferId}", transactionDTO.OfferId);

            var transaction = transactionDTO.Adapt<Transaction>();
            transaction.UserId = _accountId;
            var transactionResponse = await _transactionService.CreateAsync(transaction, cancellationToken);

            return CreatedAtAction(nameof(GetMyTransaction), new { id = transactionResponse.Id }, transactionResponse);
        }

        [HttpPatch("deposit")]
        public async Task<IActionResult> Deposit(DepositRequestDTO depositDTO, CancellationToken cancellationToken)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => new DepositCouldNotValidateException("Validation failed", errors));

            _logger.LogInformation("attempt to make a Deposit");
            await _accountService.DepositAsync(_accountId, depositDTO.Amount, cancellationToken);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var currentUser = await _accountService.GetUserAsync(_accountId, cancellationToken);
            return Ok(currentUser);
        }

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetMyTransaction(int id, CancellationToken cancellationToken)
        {
            var myTransaction = await _transactionService.GetMyTransactionAsync(id, _accountId, cancellationToken);
            return Ok(myTransaction);
        }

        [HttpDelete("transactions/{id}")]
        public async Task<IActionResult> RefundTransaction(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("attempt to refund a transaction");
            await _transactionService.RefundAsync(id, _accountId, cancellationToken);

            return NoContent();
        }

    }
}
