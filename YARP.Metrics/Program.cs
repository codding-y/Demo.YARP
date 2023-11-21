using YARP.Metrics;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddControllers();

services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

services.AddHttpContextAccessor();

// 用于收集有关代理转发的常规指标的接口
services.AddMetricsConsumer<ForwarderMetricsConsumer>();

// 将使用者注册到代理转发器遥测的事件
services.AddTelemetryConsumer<ForwarderTelemetryConsumer>();

// 将使用者注册到HttpClient遥测事件
services.AddTelemetryConsumer<HttpClientTelemetryConsumer>();

services.AddTelemetryConsumer<WebSocketsTelemetryConsumer>();

var app = builder.Build();

// 收集和报告代理度量的自定义中间件
// 放置在开头，因此它是每个请求运行的第一件也是最后一件事
app.UsePerRequestMetricCollection();

// 用于拦截WebSocket连接并收集暴露给WebSocketsTemetryConsumer的遥测的中间件
app.UseWebSocketsTelemetry();

app.MapReverseProxy();

app.Run();