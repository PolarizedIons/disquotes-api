using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuotesLib.Models;

namespace QuotesApi.Middlewares
{
    public class ValidationProblemMiddleware : IActionResult
    {
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var modelErrors = context.ModelState.Where(x => x.Value.Errors.Count > 0);
            var errors = new List<string>();

            foreach (var stateEntry in modelErrors)
            {
                foreach (var stateError in stateEntry.Value.Errors)
                {
                    errors.Add($"{stateEntry.Key}: '{stateError.ErrorMessage}'");
                }
            }

            var errorMsg = string.Join(";", errors);
            var response = new ApiResult<object>
            {
                Data = null,
                Error = $"Validation Error ({errorMsg})",
                Exception = null,
                Status = (int) HttpStatusCode.BadRequest,
            };
            context.HttpContext.Response.StatusCode = response.Status;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
