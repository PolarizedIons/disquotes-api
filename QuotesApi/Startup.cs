using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using QuotesApi.Database;
using QuotesApi.Extentions;
using QuotesApi.Filters;
using QuotesApi.Middlewares;
using QuotesApi.Models;
using QuotesApi.RouteConstraints;
using QuotesApi.Services;
using Serilog;

namespace QuotesApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("Quotes"));
            });

            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add(ULongConstraint.Name, typeof(ULongConstraint));
            });
            
            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

            services.AddJwtTokenAuth(Configuration["Jwt:Secret"]);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "QuotesAPI"});
                
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "QuotesApi.xml"));

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer keyboardcat\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                });
                
                options.OperationFilter<AddAuthHeaderOperationFilter>();
            });

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                              builder =>
                              {
                                  builder.WithOrigins(Configuration["AllowedHosts"])
                                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                                    .AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader();
                              });
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ctx => new ValidationProblemMiddleware();
            });

            services.AddSingleton<HttpClient>();
            services.DiscoverAndMakeDiServicesAvailable();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext databaseContext, DiscordService discordService)
        {
            app.UseSerilogRequestLogging();
            
            Log.Information("Migrating Database...");
            databaseContext.Database.Migrate();

            Log.Information("Logging into Discord");
            discordService.Login().Wait();

            if (env.IsDevelopment())
            {
                ApiResultSettings.ExposeExceptions = true;
            }

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var response = (ApiResult<object>) exceptionFeature.Error;
                    context.Response.StatusCode = response.Status;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                });
            });

            app.UseMiddleware<IgnoreMyHttpExceptions>();

            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var response = (ApiResult<object>) new ObjectResult(null) { StatusCode = context.HttpContext.Response.StatusCode };
                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            });
            
            // Caching
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl = 
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(30)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new[] { "Accept-Encoding" };

                await next();
            });

            app.UseRouting()
                .UseCors()
                .UseResponseCompression()
                .UseAuthentication()
                .UseAuthorization()
                .UseSwagger()
                .UseSwaggerUI(o =>
                {
                    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Quotes API");
                })
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
