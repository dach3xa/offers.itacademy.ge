using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Guest;
using offers.Application.Services.Offers;
using offers.Application.Services.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.User
{
    public class GetMyTransactionsHandler : IRequestHandler<GetMyTransactionsQuery, List<TransactionResponseModel>>
    {
        private readonly ITransactionService _transactionService;

        public GetMyTransactionsHandler(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<List<TransactionResponseModel>> Handle(GetMyTransactionsQuery request, CancellationToken cancellationToken)
        {
            var transactionResponses = await _transactionService.GetMyTransactionsAsync(request.AccountId, request.PageNumber, request.PageSize, cancellationToken);
            return transactionResponses;
        }
    }
}
