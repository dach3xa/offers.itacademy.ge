﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.API.Infrastructure.Middlewares;
using offers.Application.Models.Response;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Domain.Enums;

namespace offers.API.Controllers.V1
{
    [ApiController]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class GuestController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IOfferService _offerService;

        public GuestController(IOfferService offerService, ICategoryService categoryService)
        {
            _offerService = offerService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retrieves all available categories.
        /// </summary>
        /// <returns>
        /// A 200 OK response with a list of all categories.
        /// </returns>
        /// <response code="200">Returns a list of all categories</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CategoryResponseModel>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategoriesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellation = default)
        {
            var categories = await _categoryService.GetAllAsync(pageNumber, pageSize, cancellation);

            return Ok(categories);
        }

        /// <summary>
        /// Retrieves a specific category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to retrieve.</param>
        /// <returns>
        /// A 200 OK response with the category details,
        /// or a 404 Not Found response if the category does not exist.
        /// </returns>
        /// <response code="200">Returns the requested category</response>
        /// <response code="404">Category not found (CategoryNotFoundException)</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryResponseModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById([FromRoute]int id, CancellationToken cancellation)
        {
            var category = await _categoryService.GetAsync(id, cancellation);

            return Ok(category);
        }

        /// <summary>
        /// Retrieves all available offers.
        /// </summary>
        /// <returns>
        /// A 200 OK response with a list of all offers.
        /// </returns>
        /// <response code="200">Returns a list of all offers</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OfferResponseModel>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("offers")]
        public async Task<IActionResult> GetAllOffersAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellation = default)
        {
            var offers = await _offerService.GetAllAsync(pageNumber, pageSize, cancellation);

            return Ok(offers);
        }

        /// <summary>
        /// Retrieves a specific offer by its ID.
        /// </summary>
        /// <param name="id">The ID of the offer to retrieve.</param>
        /// <returns>
        /// A 200 OK response with the offer details,
        /// or a 404 Not Found response if the offer does not exist.
        /// </returns>
        /// <response code="200">Returns the requested offer</response>
        /// <response code="404">Offer not found (OfferNotFoundException)</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OfferResponseModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("offers/{id}")]
        public async Task<IActionResult> GetOfferById([FromRoute]int id, CancellationToken cancellation)
        {
            var offer = await _offerService.GetAsync(id, cancellation);

            return Ok(offer);
        }

    }
}
