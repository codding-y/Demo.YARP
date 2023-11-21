using System;
using Yarp.Telemetry.Consumption;

namespace YARP.Metrics
{
    /// <summary>
    /// 已转发请求的实时信息和自上次度量标准快照以来经过的时间的新请求的数量
    /// </summary>
    public sealed class ForwarderMetricsConsumer : IMetricsConsumer<ForwarderMetrics>
    {
        public void OnMetrics(ForwarderMetrics previous, ForwarderMetrics current)
        {
            var elapsed = current.Timestamp - previous.Timestamp;
            var newRequests = current.RequestsStarted - previous.RequestsStarted;
            Console.Title = $"Forwarded {current.RequestsStarted} requests ({newRequests} in the last {(int)elapsed.TotalMilliseconds} ms)";
        }
    }
}
