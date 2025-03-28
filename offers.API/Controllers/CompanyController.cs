using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.API.Controllers.Helper;
using offers.API.Models;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Token;
using offers.Application.Exceptions.Transaction;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using offers.Domain.Enums;
using offers.Domain.Models;

namespace offers.API.Controllers
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.Company))]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly ILogger<CompanyController> _logger;
        private int _accountId;

        public CompanyController(IOfferService offerService, ILogger<CompanyController> logger)
        {
            _offerService = offerService;
            _logger = logger;
            _accountId = ControllerHelper.GetUserIdFromClaims(User);
        }

        [HttpPost("offers")]
        public async Task<IActionResult> Post(OfferDTO offerDTO, CancellationToken cancellation)
        {
            ControllerHelper.ValidateModelState(
             ModelState,
             errors => throw new OfferCouldNotValidateException("Could not validate the given offer", errors));

            _logger.LogInformation("attempt to add a new Offer {Name}", offerDTO.Name);

            var offer = offerDTO.Adapt<Offer>();
            offer.AccountId = _accountId;
            await _offerService.CreateAsync(offer, cancellation);

            return StatusCode(201);
        }

        [HttpGet("offers")]
        public async Task<IActionResult> GetMyOffers(CancellationToken cancellation)
        {

            var offers = await _offerService.GetMyOffersAsync(_accountId, cancellation);

            return Ok(offers);
        }

        [HttpDelete("offers/{id}")]
        public async Task Delete(int id, CancellationToken cancellation)
        {
            _logger.LogInformation("attempt to Delete an offer with the id {id}", id);
            await _offerService.DeleteAsync(id, _accountId, cancellation);
        }
    }
}
