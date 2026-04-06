using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OrderForge.Api.ExceptionHandling;
using OrderForge.Api.Logging;
using OrderForge.Api.Security;
using OrderForge.Api;
using OrderForge.Application;
using OrderForge.Application.Common;
using OrderForge.Infrastructure;
using OrderForge.ServiceDefaults;
using OrderForge.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    var runUnderAspire = string.Equals(
        builder.Configuration["OrderForge:RunUnderAspire"],
        "true",
        StringComparison.OrdinalIgnoreCase);

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
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var authSection = builder.Configuration.GetSection("Authentication");
    var authority = authSection["Authority"];
    var audience = authSection["Audience"];

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "OrderForge API",
                Version = "v1",
                Description = "OrderForge HTTP API"
            });

        if (!string.IsNullOrWhiteSpace(authority))
        {
            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT from your identity provider (e.g. Keycloak). Use Authorize and paste the raw token."
                });
        }
    });

    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(
            AuthorizationPolicies.SupplierAdmin,
            p => p.RequireRole("SupplierAdmin"));
        options.AddPolicy(
            AuthorizationPolicies.SupplierStaff,
            p => p.RequireRole("SupplierAdmin", "SupplierViewer"));
        options.AddPolicy(
            AuthorizationPolicies.InviteUsers,
            p => p.RequireRole("SupplierAdmin", "CompanyAdmin"));
    });
    builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationFailureLoggingMiddlewareResultHandler>();

    if (!string.IsNullOrWhiteSpace(authority))
    {
        var includeJwtErrorDetails = authSection.GetValue<bool?>("IncludeErrorDetails")
            ?? builder.Environment.IsDevelopment();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                // Keycloak in Aspire/docker-compose uses http://; JwtBearer defaults to requiring HTTPS metadata.
                options.RequireHttpsMetadata = authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                options.IncludeErrorDetails = includeJwtErrorDetails;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = audience,
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    // Keycloak emits "roles"; with MapInboundClaims (default true) that maps to ClaimTypes.Role.
                    // Must match KeycloakJwtClaimsMapper and what IsInRole / RequireRole evaluate.
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal is not null)
                        {
                            KeycloakJwtClaimsMapper.Map(context.Principal);
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var log = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("OrderForge.Api.Authentication.JwtBearer");
                        log.LogWarning(
                            context.Exception,
                            "JWT validation failed — {Path} — {Message}",
                            context.Request.Path.Value,
                            context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var log = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("OrderForge.Api.Authentication.JwtBearer");
                        var authError = context.AuthenticateFailure?.Message;
                        log.LogWarning(
                            "JWT challenge — {Path} — Error: {Error}, Description: {Description}, AuthenticateFailure: {AuthFailure}",
                            context.Request.Path.Value,
                            context.Error ?? "(none)",
                            context.ErrorDescription ?? "(none)",
                            authError ?? "(none)");
                        return Task.CompletedTask;
                    }
                };
            });
    }

    var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
    if (runUnderAspire)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "Default",
                policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });
    }
    else if (corsOrigins.Length > 0)
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

    if (runUnderAspire || app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderForgeDbContext>();
        await db.Database.MigrateAsync();
        await DevelopmentDataSeeder.SeedAsync(db);
        

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderForge API v1");
        });
    }

    if (!runUnderAspire)
    {
        app.UseHttpsRedirection();
    }

    if (runUnderAspire || corsOrigins.Length > 0)
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

    app.MapDefaultEndpoints();
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
