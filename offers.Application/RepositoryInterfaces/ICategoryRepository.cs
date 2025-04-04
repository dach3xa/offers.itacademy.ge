using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.RepositoryInterfaces
{
    public interface ICategoryRepository
    {
        Task<bool> ExistsAsync(string name, CancellationToken cancellationToken);
        Task CreateAsync(Category category, CancellationToken cancellationToken);
        Task<Category?> GetAsync(int id, CancellationToken cancellationToken);
        Task<List<Category>> GetAllWithIdsAsync(List<int> ids, CancellationToken cancellationToken);
        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
    }
}
