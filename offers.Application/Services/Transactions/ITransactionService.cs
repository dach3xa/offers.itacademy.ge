﻿using offers.Application.Models.Response;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Transactions
{
    public interface ITransactionService
    {
        public Task<TransactionResponseModel> CreateAsync(Transaction transaction, CancellationToken cancellationToken);
        public Task RefundAllUsersByOfferIdAsync(int offerId, CancellationToken cancellationToken);
        public Task<TransactionResponseModel> GetMyTransactionAsync(int id, int accountId, CancellationToken cancellationToken);
        public Task RefundAsync(int id, int accountId, CancellationToken cancellationToken);
        public Task<List<TransactionResponseModel>> GetMyTransactionsAsync(int accountId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}
