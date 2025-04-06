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
using Mapster;
using offers.Application.UOF;
using System.Diagnostics.CodeAnalysis;
using offers.Application.Models.Response;

namespace offers.Application.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ICategoryRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryResponseModel> CreateAsync(Category category, CancellationToken cancellationToken)
        {
            var exists = await _repository.ExistsAsync(category.Name, cancellationToken);

            if (exists)
            {
                throw new CategoryAlreadyExistsException("A category with this name already exists");
            }

            await _repository.CreateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangeAsync(cancellationToken);

            return category.Adapt<CategoryResponseModel>();
        }

        public async Task<CategoryResponseModel> GetAsync(int id, CancellationToken cancellationToken)
        {
            var category = await _repository.GetAsync(id, cancellationToken);
            if(category == null)
            {
                throw new CategoryNotFoundException($"category with the id {id} was not found");
            }

            return category.Adapt<CategoryResponseModel>();
        }

        [ExcludeFromCodeCoverage]
        public async Task<List<CategoryResponseModel>> GetAllAsync( CancellationToken cancellationToken)
        {
            var categories = await _repository.GetAllAsync(cancellationToken);


            return categories.Adapt<List<CategoryResponseModel>>() ?? new List<CategoryResponseModel>();
        }
    }
}
