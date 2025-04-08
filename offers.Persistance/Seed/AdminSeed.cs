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
using System.Security.Claims;

namespace offers.Persistance.Seed
{
    public static class AdminSeed
    {
        public static async Task Initialize(IServiceProvider service)
        {
            using var scope = service.CreateAsyncScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Migrate(database);

            await SeedAdmin(userManager);
        }

        private static void Migrate(ApplicationDbContext context)
        {
            context.Database.Migrate();
        }

        private static async Task SeedAdmin(UserManager<Account> userManager)
        {
            string email = "randomuser@example.com";
            string password = "Dachidachi1.";

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return;

            var admin = new Account
            {
                Email = email,
                NormalizedEmail = email.ToUpper(),
                UserName = email,
                Role = AccountRole.Admin
            };

            var result = await userManager.CreateAsync(admin, password);

            if (result.Succeeded)
            {
                await userManager.AddClaimsAsync(admin, new List<Claim>()
                {
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Role, admin.Role.ToString()),
                    new Claim("id", admin.Id.ToString())
                });
            }
        }
    }
}

