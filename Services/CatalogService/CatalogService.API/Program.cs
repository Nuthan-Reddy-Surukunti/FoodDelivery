using System.Text.Json.Serialization;
using MassTransit;
using CatalogService.Application;
using CatalogService.Application.EventHandlers;
using CatalogService.Infrastructure;
using CatalogService.API.Middleware;
using FoodDelivery.Shared.Events.Catalog;
using FoodDelivery.Shared.Events.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === 1. Add Services (DI) ===

// Infrastructure services (DbContext, repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Application services (DTOs, mappings, business logic)
builder.Services.AddApplicationServices();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RestaurantApprovedEventHandler>();
    x.AddConsumer<RestaurantRejectedEventHandler>();
    x.AddConsumer<RestaurantDeletedEventHandler>();
    
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
        cfg.ConfigureEndpoints(context, new DefaultEndpointNameFormatter("CatalogService", false));
    });
});

// Middleware services
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

// Controllers with JSON naming policy
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: true));
    });

// === 2a. Endpoints API Explorer (required for Swagger in .NET 6+) ===
builder.Services.AddEndpointsApiExplorer();

// === 2. JWT Authentication ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };
    });

// === 3. CORS Configuration (enable when frontend is ready) ===
// Uncomment when integrating React frontend at http://localhost:3000
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend", policy =>
//     {
//         policy.WithOrigins("http://localhost:3000")
//             .AllowAnyMethod()
//             .AllowAnyHeader()
//             .AllowCredentials();
//     });
// });

// === 4. Swagger/OpenAPI ===
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Catalog Service", Version = "v1" });

    // Add JWT support in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
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

builder.Services.AddAuthorization();

// === 5. Build the app ===
var app = builder.Build();

// === 6. Configure Middleware Pipeline ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Register global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// app.UseCors("AllowFrontend"); // Enable when frontend is integrated

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// === 7. Run app ===
app.Run();

