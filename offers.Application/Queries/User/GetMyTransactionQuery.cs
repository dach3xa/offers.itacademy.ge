using MediatR;
using offers.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.User
{
    public record GetMyTransactionQuery(int Id, int AccountId) : IRequest<TransactionResponseModel>
    {
    }
}
