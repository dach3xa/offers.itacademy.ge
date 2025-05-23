using FluentValidation;
using offers.API.Infrastructure.Extensions;
using System.Reflection;
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
using Swashbuckle.AspNetCore.Filters;
using offers.API.Infrastructure.Swagger.Examples;
using Asp.Versioning;
using offers.API.Infrastructure.Middlewares;
using Asp.Versioning.Conventions;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using offers.Domain.Models;
using offers.Application.Mappings;
using offers.Application.Validators;
using offers.Application.ServicesExtension;
using offers.Infrastructure.RepositoryExtension;
using HealthChecks.UI;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.SqlServer;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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

builder.Services.AddHealthChecks()
.AddSqlServer(
    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    healthQuery: "SELECT 1",
    name: "SQL Server",
    failureStatus: HealthStatus.Unhealthy,
    tags: new[] { "db", "sql" }
);

builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(60);
    opt.MaximumHistoryEntriesPerEndpoint(60);
    opt.SetApiMaxActiveRequests(1);
    opt.AddHealthCheckEndpoint("API", "/api/health");
}).AddInMemoryStorage();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Offers API",
        Version = "v1",
        Description = "an Api for User Management",
        Contact = new OpenApiContact
        {
            Name = "dachi",
            Email = "kirvalidzedachi@gmail.com",
            Url = new Uri("https://github.com/dachi")
        }
    });

    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Offers API",
        Version = "v2",
        Description = "an Api for User Management",
        Contact = new OpenApiContact
        {
            Name = "dachi",
            Email = "kirvalidzedachi@gmail.com",
            Url = new Uri("https://github.com/dachi")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' [space] and then your valid JWT token.\nExample: Bearer abc123...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.OperationFilter<AuthOperationFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
    c.ExampleFilters();
});

builder.Services.AddApiVersioning(options =>
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

builder.Services.AddIdentityCore<Account>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager();


builder.Services.AddSwaggerExamplesFromAssemblyOf<CategoryDTOMultipleExamples>();

builder.Services.AddServices();
builder.Services.AddRepositories();

builder.Services.AddValidatorsFromAssemblyContaining<AccountLoginDTOValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddProblemDetails();

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));
builder.Services.Configure<JWTConfiguration>(builder.Configuration.GetSection(nameof(JWTConfiguration)));

builder.Services.AddTokenAuthentication(builder.Configuration.GetSection(nameof(JWTConfiguration)).GetSection(nameof(JWTConfiguration.Secret)).Value);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection))));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OfferDeletedEventHandler).Assembly));

builder.Services.AddHostedService<OfferArchivingService>();

builder.Services.RegisterMaps();

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionHandler>();
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"/swagger/v1/swagger.json", $"v1");
        c.SwaggerEndpoint($"/swagger/v2/swagger.json", $"v2");
    });
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("starting web host");
    await AdminSeed.Initialize(app.Services);
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

