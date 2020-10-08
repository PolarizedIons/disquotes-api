using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NATS.Client;
using QuotesApi.Exceptions;
using QuotesLib.Models;

namespace QuotesApi.Middlewares
{
    public class IgnoreMyHttpExceptions
    {
        private readonly RequestDelegate _next;

        public IgnoreMyHttpExceptions(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            { 
                var response = (ApiResult<object>) e;
                if (! (e is HttpException || e is NATSTimeoutException))
                {
                    response.Error = "Internal Server Error";
                    response.Status = 500;
                }

                context.Response.StatusCode = response.Status;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase}));
            }
        }
    }
}
