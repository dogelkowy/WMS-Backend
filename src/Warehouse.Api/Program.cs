using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;
using Scalar.AspNetCore;
using Warehouse.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("WarehouseDb")
    ?? throw new InvalidOperationException(
        "Connection string 'WarehouseDb' not found.");

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key not found.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            NameClaimType = "username",
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<WarehouseDbContext>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();

    await AdminSeeder.SeedAsync(
        context,
        configuration,
        passwordHasher
    );
}

// Middleware
app.UseHttpsRedirection();

app.UseCors("Angular");

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();