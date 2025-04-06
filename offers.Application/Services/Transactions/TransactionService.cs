using Mapster;
using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Account.User;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Refund;
using offers.Application.Exceptions.Transaction;
using offers.Application.Models.Response;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using offers.Application.UOF;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IOfferRepository _offerRepository;

        private readonly IAccountService _accountService;
        private readonly IOfferService _offerService;

        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IOfferRepository offerRepository, IOfferService offerService, IAccountService accountService, IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _offerRepository = offerRepository;

            _offerService = offerService;
            _accountService = accountService;

            _unitOfWork = unitOfWork;
        }

        public async Task<TransactionResponseModel> CreateAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            await PopulateTransaction(transaction, cancellationToken);

            ValidateCreateTransactionBusinessRules(transaction, cancellationToken);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _accountService.WithdrawAsync(transaction.UserId, transaction.Paid, cancellationToken);
                await _offerService.DecreaseStockAsync(transaction.OfferId, transaction.Count, cancellationToken);
                await _transactionRepository.CreateAsync(transaction, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new TransactionCouldNotBeCreatedException("Failed to create a transaction because of an unknown issue");
            }

            return transaction.Adapt<TransactionResponseModel>();
        }

        private void ValidateCreateTransactionBusinessRules(Transaction transaction, CancellationToken cancellationToken)
        {
            if(transaction.Offer.Price * transaction.Count != transaction.Paid)
            {
                throw new TransactionCouldNotBeCreatedException("Paid amount does not match the expected total");
            }
            else if (transaction.Offer.IsArchived)
            {
                throw new OfferExpiredException("the offer that you are trying to access is archived");
            }
        }

        private async Task PopulateTransaction(Transaction transaction, CancellationToken cancellationToken)
        {
            var transactionAccount = await _accountRepository.GetAsync(transaction.UserId, cancellationToken);
            if (transactionAccount == null || transactionAccount.UserDetail == null)
            {
                throw new UserNotFoundException("this transaction's user account could not be found");
            }

            var transactionOffer = await _offerRepository.GetAsync(transaction.OfferId, cancellationToken);
            if (transactionOffer == null)
            {
                throw new OfferNotFoundException("this transaction's offer could not be found");
            }

            transaction.User = transactionAccount.UserDetail;
            transaction.Offer = transactionOffer;
        }

        [ExcludeFromCodeCoverage]
        public async Task RefundAllUsersByOfferIdAsync(int offerId, CancellationToken cancellationToken)
        {
            var offerTransactions = await _transactionRepository.GetByOfferIdAsync(offerId, cancellationToken);

            foreach (Transaction transaction in offerTransactions)
            {
                await _accountService.DepositAsync(transaction.UserId, transaction.Paid, cancellationToken);
            }
            await _transactionRepository.DeleteByOfferIdAsync(offerId, cancellationToken);

            await _unitOfWork.SaveChangeAsync(cancellationToken);
        }

        [ExcludeFromCodeCoverage]
        public async Task<TransactionResponseModel> GetMyTransactionAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            var transaction = await GetMyDomainTransactionAsync(id, accountId, cancellationToken);

            return transaction.Adapt<TransactionResponseModel>();
        }

        private async Task<Transaction> GetMyDomainTransactionAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetAsync(id, cancellationToken);

            if (transaction == null)
            {
                throw new TransactionNotFoundException($"Transaction with the id {id} was not found");
            }
            if (transaction.UserId != accountId)
            {
                throw new TransactionAccessDeniedException("You cannot access this Transaction because it does not belong to you");
            }

            return transaction;
        }

        public async Task RefundAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            var transaction = await GetMyDomainTransactionAsync(id,accountId,cancellationToken);

            if (transaction.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                throw new TransactionCouldNotBeRefundedException("you can only refund a transaction within 5 minutes");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _offerService.IncreaseStockAsync(transaction.OfferId, transaction.Count, cancellationToken);
                await _accountService.DepositAsync(accountId, transaction.Paid, cancellationToken);
                await _transactionRepository.DeleteAsync(id, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task<List<TransactionResponseModel>> GetMyTransactionsAsync(int accountId, CancellationToken cancellationToken)
        {
            var transactions = await _transactionRepository.GetMyTransactionsAsync(accountId, cancellationToken);

            return transactions.Adapt<List<TransactionResponseModel>>() ?? new List<TransactionResponseModel>();
        }
    }
}
