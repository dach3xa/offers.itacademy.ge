using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Company;
using offers.Application.Services.Offers;
using offers.Application.Services.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.User
{
    public class GetMyTransactionHandler : IRequestHandler<GetMyTransactionQuery, TransactionResponseModel>
    {
        private readonly ITransactionService _transactionService;

        public GetMyTransactionHandler(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<TransactionResponseModel> Handle(GetMyTransactionQuery request, CancellationToken cancellationToken)
        {
            var transactionResponse = await _transactionService.GetMyTransactionAsync(request.Id, request.AccountId, cancellationToken);
            return transactionResponse;
        }
    }
}
