using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Helper;
using offers.API.Infrastructure.Middlewares;
using offers.API.Infrastructure.Swagger.Examples;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Token;
using offers.Application.Exceptions.Transaction;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using offers.Domain.Enums;
using offers.Domain.Models;
using Swashbuckle.AspNetCore.Filters;

namespace offers.API.Controllers.V2
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.Company))]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IAccountService _accountService;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(IOfferService offerService, IAccountService accountService, ILogger<CompanyController> logger)
        {
            _offerService = offerService;
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new offer for the currently authenticated account for version 2.
        /// </summary>
        /// <param name="offerDTO">The offer data to be created.</param>
        /// <returns>
        /// A 201 Created response with the newly created offer,
        /// or an error response indicating why creation failed.
        /// </returns>
        /// <response code="201">Offer successfully created</response>
        /// <response code="400">
        /// Validation failed (OfferCouldNotValidateException),
        /// or missing/invalid input data
        /// </response>
        /// <response code="404">
        /// Account, Company, or Category not found
        /// (AccountNotFoundException, CompanyNotFoundException, CategoryNotFoundException)
        /// </response>
        /// <response code="409">
        /// Company is not active (CompanyIsNotActiveException)
        /// </response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OfferResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [SwaggerRequestExample(typeof(OfferDTO), typeof(OfferDTOMultipleExamples))]
        [HttpPost("offers")]
        public async Task<IActionResult> Post([FromBody] OfferDTO offerDTO, CancellationToken cancellation)
        {
            ControllerHelper.ValidateModelState(
                ModelState,
                errors => throw new OfferCouldNotValidateException("Could not validate the given offer", errors));

            _logger.LogInformation("attempt to add a new Offer {Name}", offerDTO.Name);

            var offer = offerDTO.Adapt<Offer>();
            offer.AccountId = ControllerHelper.GetUserIdFromClaims(User);
            var offerResponse = await _offerService.CreateAsync(offer, cancellation);

            var responseV2 = new
            {
                offerResponse,
                timestamp = DateTime.UtcNow,
                message = "Offer created successfully in v2"
            };

            return CreatedAtAction(nameof(GetMyOffer), new { id = offerResponse.Id }, responseV2);
        }

        /// <summary>
        /// Retrieves all offers created by the currently authenticated account for version 2.
        /// </summary>
        /// <returns>
        /// A 200 OK response with a list of the user's offers,
        /// or an error response if the company is not found or is inactive.
        /// </returns>
        /// <response code="200">Returns a list of offers created by the authenticated user</response>
        /// <response code="404">Company not found (CompanyNotFoundException)</response>
        /// <response code="409">Company is not active (CompanyIsNotActiveException)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OfferResponseModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("offers")]
        public async Task<IActionResult> GetMyOffers(CancellationToken cancellation)
        {
            var offers = await _offerService.GetMyOffersAsync(ControllerHelper.GetUserIdFromClaims(User), cancellation);

            var responseV2 = new
            {
                offers,
                totalCount = offers.Count(),
                message = "Here are your offers from version 2"
            };

            return Ok(responseV2);
        }

        /// <summary>
        /// Retrieves a specific offer by ID that belongs to the authenticated user for version 2.
        /// </summary>
        /// <param name="id">The ID of the offer to retrieve.</param>
        /// <returns>
        /// A 200 OK response with the requested offer,
        /// or an error response if access is denied, the offer or company is not found,
        /// or the company is inactive.
        /// </returns>
        /// <response code="200">Returns the requested offer</response>
        /// <response code="403">Access denied for this offer (OfferAccessDeniedException)</response>
        /// <response code="404">
        /// Offer or company not found
        /// (OfferNotFoundException, CompanyNotFoundException)
        /// </response>
        /// <response code="409">Company is not active (CompanyIsNotActiveException)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OfferResponseModel))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("offers/{id}")]
        public async Task<IActionResult> GetMyOffer([FromRoute] int id, CancellationToken cancellation)
        {
            var offerResponse = await _offerService.GetMyOfferAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellation);
            var responseV2 = new
            {
                offerResponse,
                message = "Company data fetched in version 2",
                timestamp = DateTime.UtcNow
            };

            return Ok(responseV2);
        }

        /// <summary>
        /// Deletes a specific offer belonging to the authenticated user for version 2.
        /// </summary>
        /// <param name="id">The ID of the offer to delete.</param>
        /// <returns>
        /// A 204 No Content response if deletion is successful,
        /// or an error response if deletion fails, access is denied,
        /// the offer/company is not found, or refunding the transaction fails.
        /// </returns>
        /// <response code="204">Offer successfully deleted</response>
        /// <response code="403">Access denied for this offer (OfferAccessDeniedException)</response>
        /// <response code="404">
        /// Offer or company not found
        /// (OfferNotFoundException, CompanyNotFoundException)
        /// </response>
        /// <response code="409">
        /// Company is not active (CompanyIsNotActiveException),
        /// deletion failed (OfferCouldNotBeDeletedException),
        /// or transaction could not be refunded (TransactionCouldNotBeRefundedException)
        /// </response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpDelete("offers/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellation)
        {
            _logger.LogInformation("Attempt to delete offer with id {id} in version 2", id);
            await _offerService.DeleteAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellation);

            return NoContent();
        }

        /// <summary>
        /// Retrieves the company associated with the currently authenticated user for version 2.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the current company information,
        /// or an error response if the company is not found.
        /// </returns>
        /// <response code="200">Returns the company associated with the user</response>
        /// <response code="404">Company not found (CompanyNotFoundException)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyResponseModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet]
        public async Task<IActionResult> GetCurrentCompany(CancellationToken cancellationToken)
        {
            var currentCompany = await _accountService.GetCompanyAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);
            var responseV2 = new
            {
                currentCompany,
                message = "Company data fetched in version 2",
                timestamp = DateTime.UtcNow
            };

            return Ok(responseV2);
        }

    }
}
