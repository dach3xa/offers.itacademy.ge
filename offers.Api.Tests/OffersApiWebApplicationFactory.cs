using Microsoft.AspNetCore.Hosting;
using offers.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using offers.Persistance.Connection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace offers.Api.Tests
{
    public class OffersApiWebApplicationFactory : CustomWebApplicationFactory<CustomStartup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.ConfigureServices((WebHostBuilderContext context, IServiceCollection services) =>
            {
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));
            });
        }
    }
}
