using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions.Category;
using offers.Application.Exceptions.Offer;
using offers.Application.Exceptions.Refund;
using offers.Application.Exceptions.Transaction;
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

        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IOfferRepository offerRepository, IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _offerRepository = offerRepository;

            _unitOfWork = unitOfWork;
            
            _logger = logger;
        }

        public async Task CreateAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            await PopulateTransaction(transaction, cancellationToken);

            ValidateTransactionBusinessRules(transaction, cancellationToken);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _accountService.WithdrawAsync(transaction.AccountId, transaction.Paid, cancellationToken);
                await _offerService.DecreaseStockAsync(transaction.OfferId, transaction.Count, cancellationToken);

                var isAdded = await _transactionRepository.CreateAsync(transaction, cancellationToken);

                if (!isAdded)
                {
                    _logger.LogError("Failed to create a transaction, unknown issue");
                    throw new TransactionCouldNotBeCreatedException("Failed to create a transaction because of an unknown issue");
                }
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private void ValidateTransactionBusinessRules(Transaction transaction, CancellationToken cancellationToken)
        {
            //if(transaction.Paid > transaction.Account.UserDetail!.Balance)
            //{
            //    _logger.LogError("Failed to create a transaction, due to insufficient funds");
            //    throw new TransactionInvalidAmountException("you can't pay more than you have!");
            //}
            if(transaction.Offer.Price * transaction.Count != transaction.Paid)
            {
                _logger.LogError("Failed to create a transaction, paid amount doesnt match the excpected amount");
                throw new TransactionInvalidAmountException("Paid amount does not match the expected total");
            }
            else if (transaction.Offer.IsArchived)
            {
                _logger.LogError("Failed to create a transaction, offer is archived");
                throw new OfferExpiredException("the offer that you are trying to access is archived");
            }
        }

        private async Task PopulateTransaction(Transaction transaction, CancellationToken cancellationToken)
        {
            var transactionAccount = await _accountRepository.GetAsync(transaction.AccountId);
            if (transactionAccount == null)
            {
                _logger.LogError("Failed to create a transaction, transaction's Account could not be found");
                throw new AccountNotFoundException("this transaction's account could not be found");
            }

            var transactionOffer = await _offerRepository.GetAsync(transaction.OfferId);
            if (transactionOffer == null)
            {
                _logger.LogError("Failed to create a transaction, transaction's offer could not be found");
                throw new OfferNotFoundException("this transaction's offer could not be found");
            }

            transaction.Account = transactionAccount;
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

                var IsDeleted = await _transactionRepository.DeleteByOfferIdAsync(offerId, cancellationToken);
                if (!IsDeleted)
                {
                    _logger.LogError("Failed to refund Users for offer {id} due to an unknown error", offerId);
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
    }
}
