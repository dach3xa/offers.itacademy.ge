using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.User
{
    public record DepositCommand(int AccountId, decimal Ammount) : IRequest<Unit>
    {
    }
}
