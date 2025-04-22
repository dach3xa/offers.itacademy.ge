using MediatR;
using offers.Application.Commands.Admin;
using offers.Application.Models.Response;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Admin
{
    public record GetAllUsersQuery(int PageNumber, int PageSize) : IRequest<List<UserResponseModel>>
    {
    }
}
