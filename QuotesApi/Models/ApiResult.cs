using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Extentions;

namespace QuotesApi.Models
{
    public static class ApiResultSettings
    {
        public static bool ExposeExceptions { get; set; }
    }
    public class ApiResult<T> : ActionResult
    {
        public bool Success => Status <= 399;
        public int Status { get; set; }
        public string Error { get; set; }
        public string Exception { get; set; }
        public T Data { get; set; }

        public static implicit operator ApiResult<T>(ObjectResult objectResult)
        {
            var status = objectResult.StatusCode ?? 500;
            string errorMsg = null;
            if (status >= 400 && status <= 599)
            {
                errorMsg = ((HttpStatusCode)status).ToString().SplitTitleCase();

                if (objectResult.Value != null)
                {
                    errorMsg += ": " + objectResult.Value;
                }
            }

            return new ApiResult<T>
            {
                Status = status,
                Error = errorMsg,
                Exception = null,
                Data = string.IsNullOrEmpty(errorMsg) ? (T)objectResult.Value : default,
            };
        }

        public static implicit operator ApiResult<T>(Exception exception)
        {
            return new ApiResult<T>
            {
                Status = 500,
                Error = exception.Message,
                Exception = ApiResultSettings.ExposeExceptions ? exception.ToString() : "[Hidden Exception]",
                Data = default,
            };
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            // All this to set the response status -_-
            context.HttpContext.Response.StatusCode = Status;
            context.HttpContext.Response.ContentType = "application/json";
            var json = JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.HttpContext.Response.Body.WriteAsync(json);
        }
    }
}
