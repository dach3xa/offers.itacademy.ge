using offers.Domain.Models;
using offers.Infrastructure.Repositories;
using offers.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Infrastructure.Repos
{
    public class AccountRepository : BaseRepository<Account>
    {
        public AccountRepository(ApplicationDbContext context) : base(context) { }


    }
}
