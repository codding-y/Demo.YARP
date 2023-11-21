using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace YARP.Metrics
{
    /// <summary>
    ///  收集YARP度量并在每个请求结束时记录它们
    /// </summary>
    public class PerRequestYarpMetricCollectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerRequestYarpMetricCollectionMiddleware> _logger;

        public PerRequestYarpMetricCollectionMiddleware(RequestDelegate next, ILogger<PerRequestYarpMetricCollectionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.StartTime = DateTime.UtcNow;

            await _next(context);

            // 在其他中间件步骤完成后调用
            // 通过ILogger将信息写入控制台。在你可能想要的生产场景中将结果直接写入遥测系统
            _logger.LogInformation("PerRequestMetrics: "+ metrics.ToJson());
        }
    }

    public static class YarpMetricCollectionMiddlewareHelper
    {
        public static IApplicationBuilder UsePerRequestMetricCollection(
          this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerRequestYarpMetricCollectionMiddleware>();
        }
    }
}

