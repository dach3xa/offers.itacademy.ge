using Microsoft.EntityFrameworkCore;
using offers.Application.RepositoryInterfaces;
using offers.Domain.Models;
using offers.Infrastructure.Repositories.Base;
using offers.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }
        public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken)
        {
            return await base.AnyAsync(ctg => ctg.Name == name, cancellationToken);
        }

        public async Task<List<Category>> GetAllWithIdsAsync(List<int> ids, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _dbSet
            .Where(ctg => ids.Contains(ctg.Id))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false); ;
        }

        public async Task<Category?> GetAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbSet.SingleOrDefaultAsync(cat => cat.Id == id, cancellationToken);
        }

        public async Task CreateAsync(Category category, CancellationToken cancellationToken)
        {
            await base.CreateAsync(category, cancellationToken);
        }

        public async Task<List<Category>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await base.GetAllAsync(pageNumber, pageSize, cancellationToken);
        }

        public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
