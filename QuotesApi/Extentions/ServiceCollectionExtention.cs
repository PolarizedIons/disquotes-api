using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Services;

namespace QuotesApi.Extentions
{
    public static class ServiceCollectionExtention
    {
        public static IServiceCollection AddJwtTokenAuth(this IServiceCollection services, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = JwtService.Issuer,
                        ValidAudience = JwtService.Audience,
                        ClockSkew = TimeSpan.FromMinutes(2),
                    };
                });

            return services;
        }

        public static IServiceCollection DiscoverAndMakeDiServicesAvailable(this IServiceCollection services)
        {
            var discoveredTypes = GetTypesInAssembly(typeof(IDiService));
            if (discoveredTypes != null)
            {
                foreach (var serviceType in discoveredTypes)
                {
                    if (typeof(IScopedDiService).IsAssignableFrom(serviceType))
                    {
                        services.AddScoped(serviceType);
                    }
                    else if (typeof(ISingletonDiService).IsAssignableFrom(serviceType))
                    {
                        services.AddSingleton(serviceType);
                    }
                    else
                    {
                        throw new InvalidConstraintException("Unknown type of DI Service found! " + serviceType); 
                    }
                }
            }

            return services;
        }

        private static IEnumerable<Type> GetTypesInAssembly(Type type)
        {
            return Assembly.GetEntryAssembly()
                ?.GetTypes()
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        }
    }
}
