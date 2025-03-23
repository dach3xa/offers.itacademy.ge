using offers.Application.Services.Accounts;

namespace offers.API.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
        }
    }
}
