using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using QuickBite.Gateway.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var ocelotConfigFile = builder.Environment.IsEnvironment("Docker")
    ? "ocelot.Docker.json"
    : "ocelot.json";

builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing in gateway configuration.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("Bearer", options =>
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

// Configure CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddOcelot(builder.Configuration);

// Add endpoints explorer (required by SwaggerForOcelot)
builder.Services.AddEndpointsApiExplorer();

// Configure SwaggerForOcelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("QuickBite Gateway started. Environment: {Environment}", app.Environment.EnvironmentName);
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogInformation("QuickBite Gateway stopping.");
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    app.Logger.LogInformation("QuickBite Gateway stopped.");
});

// Cookie to Bearer Token Middleware
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["jwt"];
    if (!string.IsNullOrEmpty(token) && !context.Request.Headers.ContainsKey("Authorization"))
    {
        context.Request.Headers.Add("Authorization", "Bearer " + token);
    }
    await next();
});

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowLocalhost");

app.MapWhen(context => context.Request.Path == "/", rootApp =>
{
    rootApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message = "QuickBite Gateway is running",
            hint = "Use /gateway/auth, /gateway/catalog, /gateway/orders, /gateway/admin or visit /swagger"
        });
    });
});

app.MapWhen(context => context.Request.Path == "/health", healthApp =>
{
    healthApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { status = "Healthy" });
    });
});

app.MapWhen(context => context.Request.Path == "/favicon.ico", faviconApp =>
{
    faviconApp.Run(context =>
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return Task.CompletedTask;
    });
});

// Configure SwaggerForOcelot UI  
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
    });
}

await app.UseOcelot();

app.Run();
