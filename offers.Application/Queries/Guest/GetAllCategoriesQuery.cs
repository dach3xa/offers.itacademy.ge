using MediatR;
using offers.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Guest
{
    public record GetAllCategoriesQuery(int PageNumber, int PageSize) : IRequest<List<CategoryResponseModel>>
    {
    }
}
