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
        Task<bool> Exists(string name, CancellationToken cancellationToken);
        Task<bool> CreateAsync(Category category, CancellationToken cancellationToken);
    }
}
