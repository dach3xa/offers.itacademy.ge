using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using offers.API.Infrastructure.Extensions;
using System.Reflection;
using offers.API.Infrastructure.Mappings;
using offers.API.Infrastructure.Auth.JWT;
using Serilog;
using offers.Application.BackgroundServices;
using offers.Persistance.Context;
using offers.Persistance.Connection;
using Microsoft.EntityFrameworkCore;
using offers.Persistance.Seed;
using offers.Application.Services.Offers.Events;
using Microsoft.OpenApi.Models;
using offers.API.Infrastructure.Swagger;
using offers.API.Infrastructure.ExceptionHandler;
using Swashbuckle.AspNetCore.Filters;
using offers.API.Infrastructure.Swagger.Examples;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    c.ExampleFilters();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' [space] and then your valid JWT token.\nExample: Bearer abc123...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.OperationFilter<AuthOperationFilter>();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<CategoryDTOMultipleExamples>();

builder.Services.AddServices();

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddProblemDetails();

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));
builder.Services.Configure<JWTConfiguration>(builder.Configuration.GetSection(nameof(JWTConfiguration)));

builder.Services.AddTokenAuthentication(builder.Configuration.GetSection(nameof(JWTConfiguration)).GetSection(nameof(JWTConfiguration.Secret)).Value);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection))));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OfferDeletedEventHandler).Assembly));

builder.Services.AddHostedService<OfferArchivingService>();

builder.Services.RegisterMaps();


builder.Services.AddExceptionHandler<CustomGlobalExceptionHandler>();

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


try
{
    Log.Information("starting web host");
    AdminSeed.Initialize(app.Services);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

