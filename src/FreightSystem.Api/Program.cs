using FreightSystem.Application.Interfaces;
using FreightSystem.Api.Filters;
using FreightSystem.Api.Middlewares;
using FreightSystem.Api.Services;
using FreightSystem.Infrastructure.Persistence;
using FreightSystem.Infrastructure.Repositories;
using FreightSystem.Infrastructure.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<FreightDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=FreightSystemDb;Trusted_Connection=True;"));

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ShipmentMonitoringService>();
builder.Services.AddScoped<IRateLimitService, RateLimitService>();
builder.Services.AddScoped<IApiKeyManager, ApiKeyManager>();
builder.Services.AddSingleton<IEventBus, KafkaEventBus>();
builder.Services.AddHttpClient();

builder.Services.AddSignalR();
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});
builder.Services.AddHangfireServer();

var jwtKey = builder.Configuration.GetValue<string>("JwtSettings:Secret") ?? "default_super_secure_key_please_change";
var jwtIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer") ?? "FreightSystem";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("OperationPolicy", policy => policy.RequireRole("Operation"));
    options.AddPolicy("AccountantPolicy", policy => policy.RequireRole("Accountant"));
    options.AddPolicy("SalesPolicy", policy => policy.RequireRole("Sales"));
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Version = "v1",
        Title = "Freight System API",
        Description = "نظام إدارة الشحنات والخدمات اللوجستية (Freight Forwarding & Logistics) \n\nEnglish: Freight Forwarding & Logistics Management API.",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Name = "Freight System Team",
            Email = "support@freightsystem.example",
            Url = new Uri("https://example.com")
        }
    });

    options.OperationFilter<XDescriptionOperationFilter>();
});

var app = builder.Build();

// Seed database with default roles and users
await DbInitializer.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Freight System API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHangfireDashboard();
app.UseHttpsRedirection();
app.UseTenant();
app.UseRateLimit();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditLog();
app.MapControllers();
app.MapHub<FreightSystem.Api.Hubs.LiveTrackingHub> ("/hubs/tracking");

RecurringJob.AddOrUpdate<ShipmentMonitoringService>("overdue-shipment-alerts", x => x.SendOverdueShipmentAlertsAsync(), "0 2 * * *");


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
