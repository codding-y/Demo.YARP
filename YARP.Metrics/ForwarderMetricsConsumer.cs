using System;
using Yarp.Telemetry.Consumption;

namespace YARP.Metrics
{
    /// <summary>
    /// ��ת�������ʵʱ��Ϣ�����ϴζ�����׼��������������ʱ��������������
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
