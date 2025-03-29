using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Application.Services.Transactions;

namespace offers.API.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IAccountRepository, >();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<ITransactionRepository>();
        }
    }
}
