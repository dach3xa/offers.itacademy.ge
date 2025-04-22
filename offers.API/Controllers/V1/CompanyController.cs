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
using Microsoft.AspNetCore.Hosting;
using offers.Application.FileSaver;
using MediatR;
using offers.Application.Commands.Company;
using offers.Application.Queries.Company;
using offers.Application.Queries.Admin;

namespace offers.API.Controllers.V1
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.Company))]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class CompanyController : ControllerBase
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly IMediator _mediator;

        public CompanyController(IMediator mediator, ILogger<CompanyController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new offer for the currently authenticated account.
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
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OfferResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [SwaggerRequestExample(typeof(OfferDTO), typeof(OfferDTOMultipleExamples))]
        [HttpPost("offers")]
        public async Task<IActionResult> Post([FromForm] OfferDTO offerDTO, CancellationToken cancellation)
        {
            ControllerHelper.ValidateModelState(
             ModelState,
             errors => throw new OfferCouldNotValidateException("Could not validate the given offer", errors));

            _logger.LogInformation("attempt to add a new Offer {Name}", offerDTO.Name);
            string photoUrl = "";
            if (offerDTO.Photo != null)
            {
                photoUrl = await UploadedFileSaver.SaveUploadedFileAsync(offerDTO.Photo, cancellation);
            }
            else
            {
                photoUrl = "/uploads/company-placeholder.jpg";
            }
            var offer = offerDTO.Adapt<Offer>();
            offer.AccountId = ControllerHelper.GetUserIdFromClaims(User);
            offer.PhotoURL = photoUrl;
            var offerResponse = await _mediator.Send(new CreateOfferCommand(offer), cancellation);

            return CreatedAtAction(nameof(GetMyOffer), new { id = offerResponse.Id }, offerResponse);
        }

        /// <summary>
        /// Retrieves all offers created by the currently authenticated account.
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
        public async Task<IActionResult> GetMyOffers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellation = default)
        {
            var offers = await _mediator.Send(new GetMyOffersQuery(ControllerHelper.GetUserIdFromClaims(User), pageNumber, pageSize), cancellation);

            return Ok(offers);
        }

        /// <summary>
        /// Retrieves a specific offer by ID that belongs to the authenticated user.
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
            var offerResponse = await _mediator.Send(new GetMyOfferQuery(id, ControllerHelper.GetUserIdFromClaims(User)));

            return Ok(offerResponse);
        }

        /// <summary>
        /// Deletes a specific offer belonging to the authenticated user.
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
            _logger.LogInformation("attempt to Delete an offer with the id {id}", id);
            await _mediator.Send(new DeleteOfferCommand(id, ControllerHelper.GetUserIdFromClaims(User)), cancellation);

            return NoContent();
        }

        /// <summary>
        /// Changes the profile picture of the company belonging to the authenticated user.
        /// </summary>
        /// <param name="changePicture">The DTO containing the new picture file.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>
        /// A 204 No Content response if the picture is successfully changed,
        /// or an error response if validation fails, the company is not found,
        /// or the file could not be saved.
        /// </returns>
        /// <response code="204">Picture successfully changed</response>
        /// <response code="400">Invalid or missing file in request</response>
        /// <response code="404">Company not found (CompanyNotFoundException)</response>
        /// <response code="409">File could not be saved (FileCouldNotBeAddedException)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpPatch("change-picture")]
        public async Task<IActionResult> ChangePictureCurrentCompany([FromForm] ChangePictureDTO changePicture, CancellationToken cancellation)
        {
            int accountId = ControllerHelper.GetUserIdFromClaims(User);
            _logger.LogInformation("attempt to change a picture for the company with the id {id}", accountId);

            string newPhotoURL = await UploadedFileSaver.SaveUploadedFileAsync(changePicture.Photo, cancellation);

            await _mediator.Send(new ChangePictureCommand(accountId, newPhotoURL), cancellation);

            return NoContent();
        }

        /// <summary>
        /// Changes the picture of a specific offer belonging to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the offer whose picture is to be changed.</param>
        /// <param name="changePicture">The DTO containing the new picture file.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>
        /// A 204 No Content response if the picture is successfully changed,
        /// or an error response if validation fails, the offer is not found,
        /// access is denied, or the file could not be saved.
        /// </returns>
        /// <response code="204">Picture successfully changed</response>
        /// <response code="400">Invalid or missing file in request</response>
        /// <response code="403">Access denied for this offer (OfferAccessDeniedException)</response>
        /// <response code="404">Offer not found (OfferNotFoundException)</response>
        /// <response code="409">File could not be saved (FileCouldNotBeAddedException)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpPatch("offers/{id}/change-picture")]
        public async Task<IActionResult> ChangePictureMyOffer([FromRoute] int id, [FromForm] ChangePictureDTO changePicture, CancellationToken cancellation)
        {
            int accountId = ControllerHelper.GetUserIdFromClaims(User);

            _logger.LogInformation("attempt to change a picture of an offer with the ID {id}", id);

            string newPhotoURL = await UploadedFileSaver.SaveUploadedFileAsync(changePicture.Photo, cancellation);

            await _mediator.Send(new ChangeOfferPictureCommand(id, accountId, newPhotoURL), cancellation);

            return NoContent();
        }

        /// <summary>
        /// Retrieves the company associated with the currently authenticated user.
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
            var currentCompany = await _mediator.Send(new GetCompanyQuery(ControllerHelper.GetUserIdFromClaims(User)), cancellationToken);
            return Ok(currentCompany);
        }

    }
}
