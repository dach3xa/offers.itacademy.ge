using MediatR;
using offers.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Admin
{
    public record GetUserQuery(int Id) : IRequest<UserResponseModel>
    {
    }
}
