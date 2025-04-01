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
                PasswordHash = "15C6EA984B2717BF0B9E9A51DC48451612EA380055337FBD38510A0C3E3EF16332B0DF3362E5C83DC89D6FCA336FB124FFDEF33F317391482C228FE0F63944E3",
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

