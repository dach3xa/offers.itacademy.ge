using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using offers.API.Controllers.V1;
using offers.API.Infrastructure.Middlewares;
using offers.Application.Mappings;
using offers.Application.ServicesExtension;
using offers.Infrastructure.RepositoryExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace offers.Api.Tests
{
    public class CustomStartup
    {
        public IConfiguration Configuration { get; set; }
        public IWebHostEnvironment Environment { get; set; }

        public CustomStartup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly);

            services.AddServices();
            services.AddRepositories();

            services.RegisterMaps();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().WithMetadata(new AllowAnonymousAttribute());
            });
        }

    }
}
