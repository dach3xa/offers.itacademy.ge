using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Admin
{
    public record ConfirmCompanyCommand(int Id) : IRequest<Unit>
    {
    }
}
