using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Web.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace offers.Web.Controllers
{
    public class HomeController : Controller
    {
        private ICategoryService _categoryService;
        private IOfferService _offerService;

        public HomeController(ICategoryService categoryService, IOfferService offerService)
        {
            _categoryService = categoryService;
            _offerService = offerService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                string role = User.FindFirst(ClaimTypes.Role)?.Value;
                return RedirectToAction("Home", role);
            }

            return View();
        }

        [HttpGet("categories")]
        public async Task<IActionResult> Categories(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            return View(categories);
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> Category(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryService.GetAsync(id, cancellationToken);
            return View(category);
        }

        [HttpGet("offers")]
        public async Task<IActionResult> Offers(CancellationToken cancellationToken)
        {
            var offers = await _offerService.GetAllAsync(cancellationToken);
            return View(offers);
        }

        [HttpGet("offers/{id}")]
        public async Task<IActionResult> Offer(int id, CancellationToken cancellationToken)
        {
            var offer = await _offerService.GetAsync(id, cancellationToken);
            return View(offer);
        }

        [HttpGet("/error")]
        [HttpPost("/error")]
        [HttpGet("/error/{statusCode?}")]
        public IActionResult Error(int? statusCode = null)
        {
            var model = new ErrorViewModel();

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                model.Message = exceptionFeature.Error.Message;
            }

            model.StatusCode = statusCode ?? 500;

            return View(model);
        }
    }
}
