using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QuotesApi.Exceptions;
using QuotesApi.Models;

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
            catch (HttpException e)
            {
                var response = (ApiResult<object>) e;
                context.Response.StatusCode = response.Status;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            }
        }
    }
}
