using MediatR;
using offers.Application.Models.Response;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Auth
{
    public record RegisterUserCommand(Account Account) : IRequest<AccountResponseModel>
    {
    }
}
