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
using offers.Application.UOF;

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
            var exists = await _repository.Exists(category.Name, cancellationToken);

            if (exists)
            {
                throw new CategoryAlreadyExistsException("A category with this name already exists");
            }

            try
            {
                await _repository.CreateAsync(category, cancellationToken);
                await _unitOfWork.SaveChangeAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                throw new CategoryCouldNotBeCreatedException("Failed to create a Category because of an unknown issue");
            }

            return category.Adapt<CategoryResponseModel>();
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

        public async Task<List<CategoryResponseModel>> GetAllAsync( CancellationToken cancellationToken)
        {
            var categories = await _repository.GetAllAsync(cancellationToken);


            return categories.Adapt<List<CategoryResponseModel>>() ?? new List<CategoryResponseModel>();
        }
    }
}
