using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderForge.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

var authSection = builder.Configuration.GetSection("Authentication");
var authority = authSection["Authority"];
var audience = authSection["Audience"];
builder.Services.AddAuthorization();

if (!string.IsNullOrWhiteSpace(authority))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = audience,
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = "roles"
            };
        });
}

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "Default",
            policy =>
            {
                policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
            });
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

if (corsOrigins.Length > 0)
{
    app.UseCors("Default");
}

if (!string.IsNullOrWhiteSpace(authority))
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
