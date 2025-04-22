using MediatR;
using offers.Application.Models.Response;
using offers.Application.Services.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Admin
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryResponseModel>
    {
        private readonly ICategoryService _categoryService;

        public CreateCategoryHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<CategoryResponseModel> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var categoryResponse = await _categoryService.CreateAsync(request.Category, cancellationToken);
            return categoryResponse;
        }
    }
}
