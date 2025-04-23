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
    public class GetCategoryHandler : IRequestHandler<GetCategoryQuery, CategoryResponseModel>
    {
        private readonly ICategoryService _categoryService;

        public GetCategoryHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<CategoryResponseModel> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            var categoryResponse = await _categoryService.GetAsync(request.Id, cancellationToken);
            return categoryResponse;
        }
    }
}
