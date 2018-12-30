using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kamus
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate mNext;
        private readonly ILogger<ErrorHandlingMiddleware> mLogger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            mNext = next;
            mLogger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await mNext.Invoke(httpContext);
            } catch (Exception e) {
                mLogger.LogError(e, $"Unhandled exception while processing request");
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync("server failed to handle response");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
