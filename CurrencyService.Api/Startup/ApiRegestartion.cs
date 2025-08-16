using System;
using System.Net;
using CurrencyService.Api.Middlewares;
using CurrencyService.Domain.Models;
using CurrencyService.Domain.Wrappers;
using CurrencyService.Infrasturucture.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace CurrencyService.Api.Startup;

public static class ApiRegestration
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });
        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.Configure<UsersData>(configuration.GetSection("UsersData"));

        services
                .AddHttpClient<FrankfurterCurrencyApi>()
                .AddPolicyHandler((sp, _) =>
                    HttpPolicies.CreateResiliencePolicy(
                        sp.GetRequiredService<ILogger<FrankfurterCurrencyApi>>()));

        // Add Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = ErrorResponse.FromMessage("Unauthorized access", HttpStatusCode.Unauthorized);
                            return context.Response.WriteAsJsonAsync(response);
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";

                            var response = ErrorResponse.FromMessage("Forbidden access", HttpStatusCode.Forbidden);
                            return context.Response.WriteAsJsonAsync(response);
                        }
                    };
                });

        // Add Swagger configuration
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Currency Service API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your Bearer token in the text input below.\nExample: 'er15425hsaj'"
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
                    new string[] {}
                }
            });
        });
    }

    public static void AddApiMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLogsMiddleware>();
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<RateLimitMiddleware>();
    }
}
