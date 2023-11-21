using System;
using Yarp.ReverseProxy.Forwarder;
using Yarp.Telemetry.Consumption;

namespace YARP.Metrics
{
    /// <summary>
    /// 处理forwarder各个阶段的遥测事件，记录forwarder操作相关的指标和信息。
    /// </summary>
    public sealed class ForwarderTelemetryConsumer : IForwarderTelemetryConsumer
    {
        public void OnForwarderStart(DateTime timestamp, string destinationPrefix)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.ProxyStartOffset = metrics.CalcOffset(timestamp);
        }

        public void OnForwarderStop(DateTime timestamp, int statusCode)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.ProxyStopOffset = metrics.CalcOffset(timestamp);
        }

        public void OnForwarderFailed(DateTime timestamp, ForwarderError error)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.Error = error;
        }

        public void OnContentTransferred(DateTime timestamp, bool isRequest, long contentLength, long iops, TimeSpan readTime, TimeSpan writeTime, TimeSpan firstReadTime)
        {
            var metrics = PerRequestMetrics.Current;

            if (isRequest)
            {
                metrics.RequestBodyLength = contentLength;
                metrics.RequestContentIops = iops;
            }
            else
            {
                metrics.HttpResponseContentStopOffset = metrics.CalcOffset(timestamp);
                metrics.ResponseBodyLength = contentLength;
                metrics.ResponseContentIops = iops;
            }
        }

        public void OnForwarderInvoke(DateTime timestamp, string clusterId, string routeId, string destinationId)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.RouteInvokeOffset = metrics.CalcOffset(timestamp);
            metrics.RouteId = routeId;
            metrics.ClusterId = clusterId;
            metrics.DestinationId = destinationId;
        }
    }
}
