using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using offers.Application.BackgroundServices;
using offers.Application.Mappings;
using offers.Application.Services.Offers.Events;
using offers.Application.ServicesExtension;
using offers.Application.Validators;
using offers.Domain.Models;
using offers.Infrastructure.RepositoryExtension;
using offers.Persistance.Connection;
using offers.Persistance.Context;
using offers.Persistance.Seed;
using HealthChecks.UI;
using HealthChecks.SqlServer;
using HealthChecks.UI.Client;
using System;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.RegisterMaps();
builder.Services.AddValidatorsFromAssemblyContaining<AccountLoginDTOValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.Cookie.Name = "AuthCookie";
});

builder.Services.AddIdentityCore<Account>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager();

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.LoginPath = "/account/login";
        options.Cookie.Name = "AuthCookie";
    });

builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddHealthChecks()
.AddSqlServer(
    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    healthQuery: "SELECT 1",
    name: "SQL Server",
    failureStatus: HealthStatus.Unhealthy,
    tags: new[] { "db", "sql" }
);

// UI (optional)
builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(60);
    opt.MaximumHistoryEntriesPerEndpoint(60);
    opt.SetApiMaxActiveRequests(1);
    opt.AddHealthCheckEndpoint("API", "/api/health");
}).AddInMemoryStorage();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection))));
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OfferDeletedEventHandler).Assembly));
builder.Services.AddHostedService<OfferArchivingService>();
var app = builder.Build();

app.UseExceptionHandler("/error");
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"C:\Users\misho\Desktop\Project\offers.itacademy.ge\UploadedFiles\uploads"),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await AdminSeed.Initialize(app.Services);
app.Run();
