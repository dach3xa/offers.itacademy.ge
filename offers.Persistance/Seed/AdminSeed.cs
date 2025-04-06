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
using Microsoft.AspNetCore.Identity;

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

        private static async Task SeedAdmin(UserManager<Account> userManager)
        {
            string email = "randomuser@example.com";
            string password = "dachidachi";

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return;

            var admin = new Account
            {
                Email = email,
                UserName = "Admin Dachi",
                Role = AccountRole.Admin
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                throw new Exception("Could not create admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}

