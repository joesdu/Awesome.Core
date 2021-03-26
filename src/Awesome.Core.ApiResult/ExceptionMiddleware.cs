using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Awesome.Core.ApiResult
{
    /// <summary>
    /// 全局异常中间件
    /// </summary>
    class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            logger = loggerFactory.CreateLogger<ExceptionMiddleware>();
        }
        public async Task Invoke(HttpContext context/* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                Status = HttpStatusCode.InternalServerError,
                Msg = ex.Message,
                Data = default(object)
            },
            typeof(object),
            new()
            {
                //Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
        }
    }
    /// <summary>
    /// 全局异常中间件
    /// </summary>
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalException(this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionMiddleware>();
    }
}
