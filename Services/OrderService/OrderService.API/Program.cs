using System.Text.Json.Serialization;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Middleware;
using OrderService.Application;
using OrderService.Application.Interfaces;
using OrderService.Application.Options;
using OrderService.Application.EventHandlers;
using OrderService.Application.Services;
using OrderService.Application.Consumers;
using OrderService.Application.Saga;
using OrderService.Domain.Entities;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using OrderService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.Configure<DeliveryEmailOptions>(builder.Configuration.GetSection(DeliveryEmailOptions.SectionName));
builder.Services.Configure<DeliverySettings>(builder.Configuration.GetSection("DeliverySettings"));

// Register HttpClient for inter-service calls (AuthService)
builder.Services.AddHttpClient<IDeliveryAgentSyncService, DeliveryAgentSyncService>(client =>
{
    var authServiceUrl = builder.Configuration["Services:AuthService:Url"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(authServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddMassTransit(x =>
{
    // ── Consumers ────────────────────────────────────────────────────────
    x.AddConsumer<DeliveryAgentRegisteredEventHandler>();
    x.AddConsumer<ValidateOrderConsumer>();
    x.AddConsumer<CompensateOrderConsumer>();

    // ── Saga State Machine with EF persistence ───────────────────────────
    x.AddSagaStateMachine<OrderFulfillmentSaga, OrderFulfillmentSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.AddDbContext<DbContext, OrderDbContext>((provider, optBuilder) =>
            {
                optBuilder.UseSqlServer(
                    builder.Configuration.GetConnectionString("OrderDb"));
            });
        });

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
        cfg.ConfigureEndpoints(context, new DefaultEndpointNameFormatter("OrderService", false));
    });
});
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: true));
    });
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "QuickBite Order Service API", Version = "v1" });

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

var app = builder.Build();

// Apply migrations automatically for container startup consistency.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    app.Logger.LogInformation("Applying OrderService database migrations.");
    dbContext.Database.Migrate();
    app.Logger.LogInformation("OrderService database migrations applied successfully.");
}

// Run delivery agent sync on startup
using (var scope = app.Services.CreateScope())
{
    var syncService = scope.ServiceProvider.GetRequiredService<IDeliveryAgentSyncService>();
    try
    {
        app.Logger.LogInformation("Starting delivery agent sync from AuthService.");
        var syncedCount = await syncService.SyncDeliveryAgentsFromAuthServiceAsync();
        app.Logger.LogInformation("Synced {SyncedCount} delivery agents from AuthService.", syncedCount);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Delivery agent sync failed during startup.");
    }
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("OrderService started. Environment: {Environment}", app.Environment.EnvironmentName);
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogInformation("OrderService stopping.");
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    app.Logger.LogInformation("OrderService stopped.");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
