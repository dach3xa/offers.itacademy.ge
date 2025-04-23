using MediatR;
using offers.Application.Models.Response;
using offers.Application.Queries.Admin;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Queries.Guest
{
    public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryResponseModel>>
    {
        private readonly ICategoryService _categoryService;

        public GetAllCategoriesHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<List<CategoryResponseModel>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categoryResponses = await _categoryService.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);
            return categoryResponses;
        }
    }
}
