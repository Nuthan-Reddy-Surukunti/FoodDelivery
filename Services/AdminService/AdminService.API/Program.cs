using AdminService.Infrastructure.Persistence;
using AdminService.Infrastructure.Repositories;
using AdminService.Application.Services;
using AdminService.Application.Mappings;
using AdminService.Application.Validators;
using AdminService.Domain.Interfaces;
using AdminService.API.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AdminServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=localhost;Database=AdminServiceDb;Trusted_Connection=True;TrustServerCertificate=True;"));

// Add Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Add Application Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReportService>();

// Add AutoMapper
builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<MappingProfile>();
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
