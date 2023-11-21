using System;
using Microsoft.Extensions.Logging;
using Yarp.Telemetry.Consumption;

namespace YARP.Metrics
{
    /// <summary>
    /// ��¼ WebSocket ���ӹر��¼��������Ϣ�����Ӻ��˽� WebSocket �����ܺ���Ϊ
    /// </summary>
    public sealed class WebSocketsTelemetryConsumer : IWebSocketsTelemetryConsumer
    {
        private readonly ILogger<WebSocketsTelemetryConsumer> _logger;

        public WebSocketsTelemetryConsumer(ILogger<WebSocketsTelemetryConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnWebSocketClosed(DateTime timestamp, DateTime establishedTime, WebSocketCloseReason closeReason, long messagesRead, long messagesWritten)
        {
            _logger.LogInformation($"WebSocket connection closed ({closeReason}) after reading {messagesRead} and writing {messagesWritten} messages over {(timestamp - establishedTime).TotalSeconds:N2} seconds.");
        }
    }
}
