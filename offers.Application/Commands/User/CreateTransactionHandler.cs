using MediatR;
using offers.Application.Commands.Company;
using offers.Application.Models.Response;
using offers.Application.Services.Offers;
using offers.Application.Services.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.User
{
    public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, TransactionResponseModel>
    {
        private readonly ITransactionService _transactionService;

        public CreateTransactionHandler(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<TransactionResponseModel> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var transactionResponse = await _transactionService.CreateAsync(request.Transaction, cancellationToken);
            return transactionResponse;
        }
    }
}
