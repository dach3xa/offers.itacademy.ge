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
using offers.Web.Controllers.FileSaver;
using System.Threading;

namespace offers.Web.Controllers
{
    [Authorize(Roles = nameof(AccountRole.Company))]
    public class CompanyController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CompanyController(IOfferService offerService, IAccountService accountService, IWebHostEnvironment webHostEnvironment)
        {
            _offerService = offerService;
            _accountService = accountService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("home")]
        public IActionResult Home()
        {
            return View();
        }

        [HttpGet("offers/create")]
        public IActionResult CreateOffer()
        {
            return View();
        }

        [HttpPost("offers/create")]
        public async Task<IActionResult> CreateOffer([FromForm] OfferCreateViewModel offerViewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View();

            string photoUrl = null;
            try
            {
                if (offerViewModel.Photo != null && offerViewModel.Photo.Length > 0)
                {
                    photoUrl = await UploadedFileSaver.SaveUploadedFileAsync(offerViewModel.Photo, _webHostEnvironment);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Photo", "Only image files are allowed.");
                return View(offerViewModel);
            }

            var offer = offerViewModel.Adapt<Offer>();
            offer.PhotoURL = photoUrl;

            await _offerService.CreateAsync(offer, cancellationToken);

            return RedirectToAction("offers");
        }

        [HttpGet("offers")]
        public async Task<IActionResult> GetMyOffers(CancellationToken cancellationToken)
        {
            var offers = await _offerService.GetMyOffersAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);

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
