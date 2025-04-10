using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Models.ViewModel;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using offers.Domain.Enums;
using offers.Domain.Models;
using offers.Application.Helper;
using offers.Application.FileSaver;
using System.Threading;
using offers.Application.Services.Categories;
using offers.Application.Models.DTO;

namespace offers.Web.Controllers
{
    [Authorize(Roles = nameof(AccountRole.Company))]
    [Route("company")]
    public class CompanyController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;

        public CompanyController(IOfferService offerService, IAccountService accountService, ICategoryService categoryService)
        {
            _offerService = offerService;
            _accountService = accountService;
            _categoryService = categoryService;
        }

        [HttpGet("home")]
        public async Task<IActionResult> Home(CancellationToken cancellationToken)
        {
            var company = await _accountService.GetCompanyAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);
            return View(company);
        }

        [HttpGet("offers/create")]
        public async Task<IActionResult> CreateOffer(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost("offers/create")]
        public async Task<IActionResult> CreateOffer([FromForm] OfferDTO offerViewModel, CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categories;

            if (!ModelState.IsValid)
            {
                return View(offerViewModel); 
            }

            string photoUrl = null;
            try
            {
                if (offerViewModel.Photo != null && offerViewModel.Photo.Length > 0)
                {
                    photoUrl = await UploadedFileSaver.SaveUploadedFileAsync(offerViewModel.Photo, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Photo", "Only image files are allowed.");
                return View(offerViewModel);
            }

            var offer = offerViewModel.Adapt<Offer>();
            offer.PhotoURL = photoUrl;
            offer.AccountId = ControllerHelper.GetUserIdFromClaims(User);

            await _offerService.CreateAsync(offer, cancellationToken);

            return RedirectToAction("offers");
        }

        [HttpGet("offers")]
        public async Task<IActionResult> GetMyOffers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var offers = await _offerService.GetMyOffersAsync(ControllerHelper.GetUserIdFromClaims(User), pageNumber, pageSize, cancellationToken);
            ViewBag.currentPage = pageNumber;
            ViewBag.CanGoRight = offers.Count == pageSize;

            return View(offers);
        }

        [HttpGet("offers/{id}")]
        public async Task<IActionResult> GetMyOffer( int id, CancellationToken cancellation)
        {
            var offerResponse = await _offerService.GetMyOfferAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellation);

            return View(offerResponse);
        }

        [HttpPost("offers/{id}/delete")]//called from front end
        public async Task<IActionResult> DeleteOffer(int id, CancellationToken cancellationToken)
        {
            await _offerService.DeleteAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellationToken);

            return RedirectToAction("offers");
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentCompany(CancellationToken cancellationToken)
        {
            var company = await _accountService.GetCompanyAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);
            return View(company);
        }
    }
}
