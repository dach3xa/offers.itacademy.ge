using MediatR;
using offers.Application.Services.Accounts;
using offers.Application.Services.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.User
{
    public class RefundTransactionHandler : IRequestHandler<RefundTransactionCommand, Unit>
    {
        private readonly ITransactionService _transactionService;

        public RefundTransactionHandler(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<Unit> Handle(RefundTransactionCommand request, CancellationToken cancellationToken)
        {
            await _transactionService.RefundAsync(request.Id, request.AccountId, cancellationToken);
            return Unit.Value;
        }
    }
}
