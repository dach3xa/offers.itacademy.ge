using Mapster;
using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Refund;
using offers.Application.Exceptions.Transaction;
using offers.Application.Models;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using offers.Application.UOF;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
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

        public TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IOfferRepository offerRepository, IOfferService offerService, IAccountService accountService, IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
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
            Transaction addedTransaction;
            await PopulateTransaction(transaction, cancellationToken);

            ValidateTransactionBusinessRules(transaction, cancellationToken);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _accountService.WithdrawAsync(transaction.AccountId, transaction.Paid, cancellationToken);
                await _offerService.DecreaseStockAsync(transaction.OfferId, transaction.Count, cancellationToken);

                addedTransaction = await _transactionRepository.CreateAsync(transaction, cancellationToken);

                if (addedTransaction == null)
                {
                    throw new TransactionCouldNotBeCreatedException("Failed to create a transaction because of an unknown issue");
                }
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            return addedTransaction.Adapt<TransactionResponseModel>();
        }

        private void ValidateTransactionBusinessRules(Transaction transaction, CancellationToken cancellationToken)
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
            var transactionAccount = await _accountRepository.GetAsync(transaction.AccountId, cancellationToken);
            if (transactionAccount == null || transactionAccount.UserDetail == null)
            {
                throw new AccountNotFoundException("this transaction's account could not be found");
            }

            var transactionOffer = await _offerRepository.GetAsync(transaction.OfferId, cancellationToken);
            if (transactionOffer == null)
            {
                throw new OfferNotFoundException("this transaction's offer could not be found");
            }

            transaction.User = transactionAccount.UserDetail;
            transaction.Offer = transactionOffer;
        }

        public async Task RefundAllUsersByOfferIdAsync(int offerId, CancellationToken cancellationToken)
        {
            var offerTransactions = await _transactionRepository.GetByOfferIdAsync(offerId, cancellationToken);
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (Transaction transaction in offerTransactions)
                {
                    await _accountService.DepositAsync(transaction.AccountId, transaction.Paid, cancellationToken);
                }

                var deletedTransactions = await _transactionRepository.DeleteByOfferIdAsync(offerId, cancellationToken);
                if (deletedTransactions.Count != offerTransactions.Count)
                {
                    throw new RefundFailedException("Refund failed Due to an unknown error");
                }
                await _unitOfWork.CommitAsync(cancellationToken);

            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<TransactionResponseModel> GetMyTransactionAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetAsync(id, cancellationToken);

            if (transaction == null)
            {
                throw new TransactionNotFoundException($"Transaction with the id {id} was not found");
            }
            if (transaction.AccountId != accountId)
            {
                throw new TransactionAccessDeniedException($"You cannot access this Transaction because it does not belong to you");
            }

            return transaction.Adapt<TransactionResponseModel>();
        }

        public async Task RefundAsync(int id, int accountId, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetAsync(id, cancellationToken);
            RefundBusinessRulesCheck(transaction, accountId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _offerService.IncreaseStockAsync(transaction.OfferId, transaction.Count, cancellationToken);
                await _accountService.DepositAsync(accountId, transaction.Paid, cancellationToken);

                var deletedTransaction = await _transactionRepository.DeleteAsync(id, cancellationToken);
                if (deletedTransaction == null)
                {
                    throw new TransactionCouldNotBeRefundedException("transaction could not be refunded due to an unknwon issue");
                }
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private void RefundBusinessRulesCheck(Transaction transaction, int accountId)
        {
            if (transaction == null)
            {
                throw new TransactionNotFoundException("this transaction could not be found");
            }
            if (transaction.AccountId != accountId)
            {
                throw new TransactionAccessDeniedException("you don't have an access to this transaction");
            }
            if (transaction.CreatedAt > DateTime.Now.AddMinutes(5))
            {
                throw new TransactionCouldNotBeRefundedException("you can only refund a transaction within 5 minutes");
            }
        }
    }
}
