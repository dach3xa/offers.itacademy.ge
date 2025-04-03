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
using offers.Application.Models;
using Swashbuckle.AspNetCore.Filters;
using Asp.Versioning;
using offers.API.Infrastructure.Middlewares;

namespace offers.API.Controllers.Version_1
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.User))]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class UserController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;

        private readonly ILogger<UserController> _logger;
        public UserController(ITransactionService transactionService, IOfferService offerService, IAccountService accountService, ILogger<UserController> logger)
        {
            _transactionService = transactionService;
            _offerService = offerService;
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves offers that belong to the specified categories.
        /// </summary>
        /// <param name="categoryIds">A list of category IDs to filter offers by.</param>
        /// <returns>
        /// A 200 OK response with offers from the specified categories,
        /// or a 404 Not Found response if one or more categories do not exist.
        /// </returns>
        /// <response code="200">Returns offers filtered by the given category IDs</response>
        /// <response code="404">One or more categories not found (CategoryNotFoundException)</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OfferResponseModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("offers")]
        public async Task<IActionResult> GetOffersByCategories([FromQuery] List<int> categoryIds, CancellationToken cancellationToken)
        {
            var responseOffers = await _offerService.GetOffersByCategoriesAsync(categoryIds, cancellationToken);

            return Ok(responseOffers);
        }

        /// <summary>
        /// Creates a new transaction for the authenticated user based on the selected offer.
        /// </summary>
        /// <param name="transactionDTO">The transaction details to be created.</param>
        /// <returns>
        /// A 201 Created response with the newly created transaction,
        /// or an error response if validation fails, the user is unauthorized, or a server error occurs.
        /// </returns>
        /// <response code="201">Transaction successfully created</response>
        /// <response code="400">Validation failed (TransactionCouldNotValidateException)</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="500">Internal server error </response>
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TransactionResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody]TransactionDTO transactionDTO, CancellationToken cancellationToken)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => new TransactionCouldNotValidateException("Validation failed", errors));

            _logger.LogInformation("attempt to make a new Transaction on the offer with Id {OfferId}", transactionDTO.OfferId);

            var transaction = transactionDTO.Adapt<Transaction>();
            transaction.UserId = ControllerHelper.GetUserIdFromClaims(User); ;
            var transactionResponse = await _transactionService.CreateAsync(transaction, cancellationToken);

            return CreatedAtAction(nameof(GetMyTransaction), new { id = transactionResponse.Id }, transactionResponse);
        }

        /// <summary>
        /// Adds funds to the authenticated user's account.
        /// </summary>
        /// <param name="depositDTO">The deposit amount to be added.</param>
        /// <returns>
        /// A 204 No Content response if the deposit is successful,
        /// or an error response if validation fails, the account is not found,
        /// the deposit cannot be processed, or an unexpected error occurs.
        /// </returns>
        /// <response code="204">Deposit successful</response>
        /// <response code="400">Validation failed (DepositCouldNotValidateException)</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="404">Account not found (AccountNotFoundException)</response>
        /// <response code="409">Deposit could not be processed (AccountCouldNotDepositException, InvalidOperationException)</response>
        /// <response code="500">Internal server error during deposit</response>
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpPatch("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDTO depositDTO, CancellationToken cancellationToken)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => new DepositCouldNotValidateException("Validation failed", errors));

            _logger.LogInformation("attempt to make a Deposit");
            await _accountService.DepositAsync(ControllerHelper.GetUserIdFromClaims(User), depositDTO.Amount, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Retrieves the currently authenticated user's information.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the user's details,
        /// or a 404 Not Found response if the user does not exist.
        /// </returns>
        /// <response code="200">Returns the current user's information</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="404">User not found (UserNotFoundException)</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var currentUser = await _accountService.GetUserAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);
            return Ok(currentUser);
        }

        /// <summary>
        /// Retrieves a specific transaction by ID that belongs to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the transaction to retrieve.</param>
        /// <returns>
        /// A 200 OK response with the transaction details,
        /// or an error response if the transaction is not found, access is denied, 
        /// the user is unauthorized, or a server error occurs.
        /// </returns>
        /// <response code="200">Returns the requested transaction</response>
        /// <response code="403">Access denied for this transaction (TransactionAccessDeniedException)</response>
        /// <response code="404">Transaction not found (TransactionNotFoundException)</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionResponseModel))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetMyTransaction(int id, CancellationToken cancellationToken)
        {
            var myTransaction = await _transactionService.GetMyTransactionAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellationToken);
            return Ok(myTransaction);
        }

        /// <summary>
        /// Retrieves all transactions associated with the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A 200 OK response with a list of the user's transactions,
        /// or an error response if the user is unauthorized or an unexpected server error occurs.
        /// </returns>
        /// <response code="200">Returns the list of transactions for the current user</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TransactionResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetMyTransactions(CancellationToken cancellation)
        {
            var transactions = await _transactionService.GetMyTransactionsAsync(ControllerHelper.GetUserIdFromClaims(User), cancellation);

            return Ok(transactions);
        }

        /// <summary>
        /// Refunds a specific transaction for the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the transaction to refund.</param>
        /// <returns>
        /// A 204 No Content response if the refund is successful,
        /// or an error response if the transaction, offer, or account is not found,
        /// access is denied, the stock could not be restored, the deposit failed, 
        /// the operation is invalid, the user is unauthorized, or a server error occurs.
        /// </returns>
        /// <response code="204">Transaction successfully refunded</response>
        /// <response code="403">Access denied for this transaction (TransactionAccessDeniedException)</response>
        /// <response code="404">
        /// Transaction, offer, or account not found 
        /// (TransactionNotFoundException, OfferNotFoundException, AccountNotFoundException)
        /// </response>
        /// <response code="409">
        /// Refund failed due to business rule conflict
        /// (TransactionCouldNotBeRefundedException, OfferCouldNotIncreaseStockException,
        /// AccountCouldNotDepositException, InvalidOperationException)
        /// </response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpDelete("transactions/{id}")]
        public async Task<IActionResult> RefundTransaction([FromRoute] int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("attempt to refund a transaction");
            await _transactionService.RefundAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellationToken);

            return NoContent();
        }

    }
}
