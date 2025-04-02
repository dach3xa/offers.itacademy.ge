using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Domain.Enums;

namespace offers.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class GuestController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IOfferService _offerService;

        public GuestController(IOfferService offerService, ICategoryService categoryService)
        {
            _offerService = offerService;
            _categoryService = categoryService;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategoriesAsync(CancellationToken cancellation)
        {
            var categories = await _categoryService.GetAllAsync(cancellation);

            return Ok(categories);
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById(int id, CancellationToken cancellation)
        {
            var category = await _categoryService.GetAsync(id, cancellation);

            return Ok(category);
        }

        [HttpGet("offers")]
        public async Task<IActionResult> GetAllOfferAsync(CancellationToken cancellation)
        {
            var offers = await _offerService.GetAllAsync(cancellation);

            return Ok(offers);
        }

        [HttpGet("offers/{id}")]
        public async Task<IActionResult> GetOfferById(int id,CancellationToken cancellation)
        {
            var offer = await _offerService.GetAsync(id, cancellation);

            return Ok(offer);
        }

    }
}
