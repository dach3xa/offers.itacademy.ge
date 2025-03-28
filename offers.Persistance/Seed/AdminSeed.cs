using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using offers.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using offers.Domain.Models;
using offers.Domain.Enums;

namespace offers.Persistance.Seed
{
    public static class AdminSeed
    {
        public static async Task Initialize(IServiceProvider service)
        {
            using var scope = service.CreateAsyncScope();

            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Migrate(database);

            await SeedAdmin(database);
        }

        private static void Migrate(ApplicationDbContext context)
        {
            context.Database.Migrate();
        }

        private static async Task SeedAdmin(ApplicationDbContext context)
        {
            var admin = new Account
            {
                Email = "randomuser@example.com",
                Phone = "+1-800-555-1234",
                PasswordHash = "cdff0bbc4debe3a45e4558db28385b52e30db9b5fecb110f293eb890852b40c76a461649d01280cfdb14c24b3f14d4892d021c7404aaaf77737d83f3ae6f90ee",
                Role = AccountRole.Admin
            };

            if (!context.Accounts.Any(x => x.Role == AccountRole.Admin))
            {
                context.Accounts.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}

