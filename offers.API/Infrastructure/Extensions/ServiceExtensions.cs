using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.Services.Offers;
using offers.Application.Services.Transactions;
using offers.Application.UOF;
using offers.Infrastructure.Repositories;
using offers.Persistance.UOF;

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
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
        }
    }
}
