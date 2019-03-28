using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CustomResourceDescriptorController
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class LoggingMiddleware
    {
        private readonly RequestDelegate mNext;
        private readonly ILogger<LoggingMiddleware> mLogger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            mNext = next;
            mLogger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await mNext(httpContext);
            }catch(Exception e)
            {
                mLogger.LogError($"Unhandled exception while processing request: {e}");
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync("server failed to handle response");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
