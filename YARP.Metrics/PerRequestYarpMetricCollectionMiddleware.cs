using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace YARP.Metrics
{
    /// <summary>
    ///  �ռ�YARP��������ÿ���������ʱ��¼����
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

            // �������м��������ɺ����
            // ͨ��ILogger����Ϣд�����̨�����������Ҫ�����������н����ֱ��д��ң��ϵͳ
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

