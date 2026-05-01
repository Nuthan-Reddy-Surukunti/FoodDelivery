using System.Text.Json.Serialization;
using MassTransit;
using AdminService.Infrastructure.Persistence;
using AdminService.Infrastructure.Repositories;
using AdminService.Application.Services;
using AdminService.Application.Interfaces;
using AdminService.Application.Mappings;
using AdminService.Application.Validators;
using AdminService.Application.EventHandlers;
using AdminService.Domain.Interfaces;
using AdminService.API.Middleware;
using QuickBite.Shared.Events.Auth;
using QuickBite.Shared.Events.Catalog;
using QuickBite.Shared.Events.Order;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/adminservice-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Admin Service");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

// Add DbContext
builder.Services.AddDbContext<AdminServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=localhost,1433;Database=AdminServiceDb;User Id=sa;Password=SqlServer123@;TrustServerCertificate=True;",
        sqlOptions => sqlOptions.EnableRetryOnFailure()));
// Add Repositories
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Application Services
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RestaurantCreatedEventHandler>();
    x.AddConsumer<RestaurantUpdatedEventHandler>();
    x.AddConsumer<OrderPlacedEventHandler>();
    x.AddConsumer<OrderStatusChangedEventHandler>();
    x.AddConsumer<MenuItemCreatedEventHandler>();
    x.AddConsumer<MenuItemUpdatedEventHandler>();
    x.AddConsumer<MenuItemDeletedEventHandler>();
    x.AddConsumer<UserDeletedEventHandler>();
    x.AddConsumer<UserRegisteredEventHandler>();
    x.AddConsumer<UserStatusChangedEventHandler>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost",
            ushort.Parse(builder.Configuration["RabbitMq:Port"] ?? "5672"),
            "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMq:UserName"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
            });
        
        // Configure consumer endpoints with retry policy
        cfg.UseMessageRetry(r =>
        {
            r.Exponential(5, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(100));
        });
        
        // Configure endpoints
        cfg.ConfigureEndpoints(context, new DefaultEndpointNameFormatter("AdminService", false));
    });
});
// Add AutoMapper
builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<MappingProfile>();
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RejectRestaurantRequestValidator>();

// Add HTTP Context Accessor for audit logging
builder.Services.AddHttpContextAccessor();

// Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add HTTP Context Accessor for getting current user info
builder.Services.AddHttpContextAccessor();

// Add Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "default-secret-key-minimum-32-chars-long!!!!!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "QuickBite Admin Service API", Version = "v1" });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AdminServiceDbContext>("database");

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdminServiceDbContext>();
    try
    {
        db.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying migrations");
        throw;
    }
}

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AdminServiceDbContext>();
        await AdminServiceDbContextSeed.SeedAsync(context);
        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database");
    }
}

// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Add audit middleware after authentication/authorization
app.UseMiddleware<AuditMiddleware>();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Admin Service failed to start");
}
finally
{
    Log.CloseAndFlush();
}
