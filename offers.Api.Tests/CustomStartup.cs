using Asp.Versioning.Conventions;
using Asp.Versioning;
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
using offers.Domain.Models;
using offers.Persistance.Context;
using Microsoft.AspNetCore.Identity;
using offers.Persistance.Seed;
using offers.API.Infrastructure.Auth.JWT;
using offers.Persistance.Connection;
using offers.Application.Services.Offers.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using offers.Api.Tests.Tests.helper;

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
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Version"),
                new UrlSegmentApiVersionReader());
            }).AddMvc(options =>
            {
                options.Conventions.Add(new VersionByNamespaceConvention());
            }).AddApiExplorer(options => {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OfferDeletedEventHandler).Assembly));
            services.Configure<ConnectionStrings>(Configuration.GetSection(nameof(ConnectionStrings)));
            services.Configure<JWTConfiguration>(Configuration.GetSection(nameof(JWTConfiguration)));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(Configuration["JWTConfiguration:Secret"]))
                };
            });
            services.AddServices();
            services.AddRepositories();
            services.AddIdentityCore<Account>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

            services.RegisterMaps();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().WithMetadata(new AllowAnonymousAttribute());
            });
            AdminSeed.Initialize(app.ApplicationServices).GetAwaiter().GetResult();
        }

    }
}
