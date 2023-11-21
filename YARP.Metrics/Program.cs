using YARP.Metrics;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddControllers();

services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

services.AddHttpContextAccessor();

// �����ռ��йش���ת���ĳ���ָ��Ľӿ�
services.AddMetricsConsumer<ForwarderMetricsConsumer>();

// ��ʹ����ע�ᵽ����ת����ң����¼�
services.AddTelemetryConsumer<ForwarderTelemetryConsumer>();

// ��ʹ����ע�ᵽHttpClientң���¼�
services.AddTelemetryConsumer<HttpClientTelemetryConsumer>();

services.AddTelemetryConsumer<WebSocketsTelemetryConsumer>();

var app = builder.Build();

// �ռ��ͱ������������Զ����м��
// �����ڿ�ͷ���������ÿ���������еĵ�һ��Ҳ�����һ����
app.UsePerRequestMetricCollection();

// ��������WebSocket���Ӳ��ռ���¶��WebSocketsTemetryConsumer��ң����м��
app.UseWebSocketsTelemetry();

app.MapReverseProxy();

app.Run();