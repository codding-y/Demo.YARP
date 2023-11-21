using System;
using System.Threading;
using Yarp.ReverseProxy.Forwarder;
using System.Text.Json;

namespace YARP.Metrics
{
    public class PerRequestMetrics
    {
        private static readonly AsyncLocal<PerRequestMetrics> _local = new AsyncLocal<PerRequestMetrics>();
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        // 确保我们只能通过工厂获取
        private PerRequestMetrics() { }

        /// <summary>
        /// 用于实例化或从 AsyncLocal 存储中还原指标的工厂
        /// </summary>
        public static PerRequestMetrics Current => _local.Value ??= new PerRequestMetrics();

        // 通过管道启动请求的时间
        public DateTime StartTime { get; set; }


        // 代理操作的每个部分的偏移Tics
        public float RouteInvokeOffset { get; set; }
        public float ProxyStartOffset { get; set; }
        public float HttpRequestStartOffset { get; set; }
        public float HttpConnectionEstablishedOffset { get; set; }
        public float HttpRequestLeftQueueOffset { get; set; }

        public float HttpRequestHeadersStartOffset { get; set; }
        public float HttpRequestHeadersStopOffset { get; set; }
        public float HttpRequestContentStartOffset { get; set; }
        public float HttpRequestContentStopOffset { get; set; }

        public float HttpResponseHeadersStartOffset { get; set; }
        public float HttpResponseHeadersStopOffset { get; set; }
        public float HttpResponseContentStopOffset { get; set; }

        public float HttpRequestStopOffset { get; set; }
        public float ProxyStopOffset { get; set; }

        // 关于 request 的信息
        public ForwarderError Error { get; set; }
        public long RequestBodyLength { get; set; }
        public long ResponseBodyLength { get; set; }
        public long RequestContentIops { get; set; }
        public long ResponseContentIops { get; set; }
        public string DestinationId { get; set; }
        public string ClusterId { get; set; }
        public string RouteId { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, _jsonOptions);
        }

        public float CalcOffset(DateTime timestamp)
        {
            return (float)(timestamp - StartTime).TotalMilliseconds;
        }
    }
}

