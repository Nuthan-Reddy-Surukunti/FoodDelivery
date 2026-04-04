using CatalogService.Application;
using CatalogService.Infrastructure;
using CatalogService.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === 1. Add Services (DI) ===

// Infrastructure services (DbContext, repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Application services (DTOs, mappings, business logic)
builder.Services.AddApplicationServices();

// Controllers with JSON naming policy
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// === 2. JWT Authentication ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

// === 3. CORS Configuration ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// === 4. Swagger/OpenAPI ===
builder.Services.AddSwaggerGen();

// === 5. Build the app ===
var app = builder.Build();

// === 6. Configure Middleware Pipeline ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Register global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseCors("AllowFrontend");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// === 7. Run app ===
app.Run();

