using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using offers.Application.Helper;
using offers.Application.Models.DTO;
using offers.Application.Models.ViewModel;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Application.Services.Transactions;
using offers.Domain.Enums;
using offers.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace offers.Web.Controllers
{
    [Route("user")]
    [Authorize(Roles = nameof(AccountRole.User))]
    public class UserController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly IAccountService _accountService;

        public UserController(IOfferService offerService, ITransactionService transactionService,ICategoryService categoryService, IAccountService accountService)
        {
            _offerService = offerService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _accountService = accountService;
        }

        [HttpGet("home")]
        public async Task<IActionResult> Home(CancellationToken cancellationToken)
        {
            var user = await _accountService.GetUserAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);
            return View(user);
        }

        [HttpGet("offers")]
        public async Task<IActionResult> Offers(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            return View(categories);
        }

        [HttpGet("offers/search")]
        public async Task<IActionResult> GetOffersByCategoryIds([FromQuery] List<int> ids, CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            var offers = await _offerService.GetOffersByCategoriesAsync(ids, cancellationToken);

            var vm = new OfferSearchViewModel
            {
                Categories = categories,
                Offers = offers,
                SelectedCategoryIds = ids
            };

            return View(vm);
        }

        [HttpGet("offers/{id}")]
        public async Task<IActionResult> GetOffer(int id, CancellationToken cancellationToken)
        {
            var offer = await _offerService.GetAsync(id, cancellationToken);

            return View(offer);
        }

        [HttpGet("offers/{id}/purchase")]
        public IActionResult CreateTransaction()
        {
            return View();
        }

        [HttpPost("offers/{id}/purchase")]
        public async Task<IActionResult> CreateTransaction([FromRoute]int id, [FromForm] TransactionCreateViewModel transactionViewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(transactionViewModel);
            }

            var offer = await _offerService.GetAsync(id, cancellationToken);

            var transaction = new Transaction()
            {
                Count = transactionViewModel.Count,
                Paid = transactionViewModel.Count * offer.Price,
                UserId = ControllerHelper.GetUserIdFromClaims(User),
                OfferId = id,
                CreatedAt = DateTime.UtcNow
            };

            await _transactionService.CreateAsync(transaction, cancellationToken);
            return RedirectToAction("GetOffer", new { id });
        }

        [HttpGet("deposit")]
        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromForm] DepositRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(requestDTO);
            }

            await _accountService.DepositAsync(ControllerHelper.GetUserIdFromClaims(User), requestDTO.Amount, cancellationToken);

            return RedirectToAction("home");
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var user = await _accountService.GetUserAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);
            return View(user);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetMyTransactions(CancellationToken cancellationToken)
        {
            var transactions = await _transactionService.GetMyTransactionsAsync(ControllerHelper.GetUserIdFromClaims(User), cancellationToken);

            return View(transactions);
        }

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetMyTransaction(int id, CancellationToken cancellationToken)
        {
            var transaction = await _transactionService.GetMyTransactionAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellationToken);

            return View(transaction);
        }

        [HttpPost("transactions/{id}/refund")]//called from front end
        public async Task<IActionResult> DeleteOffer(int id, CancellationToken cancellationToken)
        {
            await _transactionService.RefundAsync(id, ControllerHelper.GetUserIdFromClaims(User), cancellationToken);

            return RedirectToAction("transactions");
        }
    }
}
