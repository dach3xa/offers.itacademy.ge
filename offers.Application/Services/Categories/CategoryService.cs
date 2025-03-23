using Microsoft.Extensions.Logging;
using offers.Application.Accounts;
using offers.Application.Exceptions.Account;
using offers.Application.Exceptions;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using offers.Application.Exceptions.Category;

namespace offers.Application.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task CreateAsync(Category category, CancellationToken cancellationToken)
        {
            var exists = await _repository.Exists(category.Name, cancellationToken);

            if (exists)
            {
                _logger.LogWarning("Failed to create a category {Name}, this category already exists", category.Name);
                throw new CategoryAlreadyExistsException("A category with this name already exists");
            }

            var isAdded = await _repository.CreateAsync(category, cancellationToken);

            if (!isAdded)
            {
                _logger.LogError("Failed to create a category {Name}, unknown issue", category.Name);
                throw new CategoryCouldNotBeCreatedException("Failed to create a Category because of an unknown issue");
            }
        }
    }
}
