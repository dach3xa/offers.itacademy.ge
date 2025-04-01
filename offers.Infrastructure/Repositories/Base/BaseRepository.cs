using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace offers.Infrastructure.Repositories.Base
{
    public class BaseRepository<T> where T : class
    {

        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync(CancellationToken token)
        {
            return await _dbSet.ToListAsync(token);
        }

        public async Task<T?> GetAsync(int id, CancellationToken token)
        {
            return await _dbSet.FindAsync(id, token);
        }

        public async Task CreateAsync(T entity, CancellationToken token)
        {
            await _dbSet.AddAsync(entity, token);
        }

        public async Task DeleteAsync(int id, CancellationToken token)
        {
            var entity = await GetAsync(id, token).ConfigureAwait(false); ;
            _dbSet.Remove(entity);
        }

        public void Delete(T entity)
        {
            if (entity == null)
                return;

            _dbSet.Remove(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken token)
        {
            return await _dbSet.AnyAsync(predicate, token).ConfigureAwait(false); ;
        }

    }
}
