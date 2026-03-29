using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using OrderForge.Api.ExceptionHandling;
using OrderForge.Api.Logging;
using OrderForge.Application;
using OrderForge.Infrastructure;
using OrderForge.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();

        var seqUrl = context.Configuration["Seq:ServerUrl"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            loggerConfiguration.WriteTo.Seq(seqUrl);
        }
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

    app.UseExceptionHandler();
    app.UseMiddleware<CorrelationIdMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderForgeDbContext>();
        await db.Database.MigrateAsync();
        await DevelopmentDataSeeder.SeedAsync(db);

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

    app.UseMiddleware<UserLogContextMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (!string.IsNullOrWhiteSpace(authority))
    {
        app.UseAuthorization();
    }

    app.MapControllers();

    Log.Information("OrderForge API starting ({Environment})", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrderForge API terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
