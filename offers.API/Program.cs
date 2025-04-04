using FluentValidation;
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
using Swashbuckle.AspNetCore.Filters;
using offers.API.Infrastructure.Swagger.Examples;
using Asp.Versioning;
using offers.API.Infrastructure.Middlewares;
using Asp.Versioning.Conventions;
using Asp.Versioning.ApiExplorer;

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

var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandler>();
app.UseExceptionHandler();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
foreach (var description in provider.ApiVersionDescriptions)
{
    Log.Information("Discovered API version: {Version}", description.GroupName);
}

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

