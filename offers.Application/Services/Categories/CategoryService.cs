using Microsoft.Extensions.Logging;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using offers.Application.Exceptions.Category;
using offers.Application.RepositoryInterfaces;
using offers.Application.Models;
using Mapster;

namespace offers.Application.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<CategoryResponseModel> CreateAsync(Category category, CancellationToken cancellationToken)
        {
            var exists = await _repository.Exists(category.Name, cancellationToken);

            if (exists)
            {
                throw new CategoryAlreadyExistsException("A category with this name already exists");
            }

            var addedCategory = await _repository.CreateAsync(category, cancellationToken);

            if (addedCategory == null)
            {
                throw new CategoryCouldNotBeCreatedException("Failed to create a Category because of an unknown issue");
            }

            return addedCategory.Adapt<CategoryResponseModel>();
        }

        public async Task<CategoryResponseModel> GetAsync(int id, CancellationToken cancellationToken)
        {
            var category = _repository.GetAsync(id, cancellationToken);
            if(category == null)
            {
                throw new CategoryNotFoundException($"category with the id {id} was not found");
            }

            return category.Adapt<CategoryResponseModel>();
        }
    }
}
