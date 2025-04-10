using offers.Application.Models.Response;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Categories
{
    public interface ICategoryService
    {
        Task<CategoryResponseModel> CreateAsync(Category category, CancellationToken cancellationToken);
        Task<CategoryResponseModel> GetAsync(int id, CancellationToken cancellationToken);
        Task<List<CategoryResponseModel>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<List<CategoryResponseModel>> GetAllAsync(CancellationToken cancellationToken);
    }
}
