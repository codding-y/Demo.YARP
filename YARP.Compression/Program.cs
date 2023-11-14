using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpForwarder();

// Add services to the container.

var app = builder.Build();

// Configure our own HttpMessageInvoker for outbound calls for proxy operations
var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.GZip, // 设置响应压缩方式
    UseCookies = false,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
    ConnectTimeout = TimeSpan.FromSeconds(15),
});

// Setup our own request transform class
var transformer = new CustomTransformer(); // or HttpTransformer.Default;
var requestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

app.UseRouting();

// Configure the HTTP request pipeline.

app.MapForwarder("/{**catch-all}", "http://localhost:5047", requestConfig, transformer, httpClient);

app.Run();

/// <summary>
/// 自定义请求转换
/// </summary>
class CustomTransformer : HttpTransformer
{
    ///<summary>
    /// 该函数在发送被代理的请求之前被调用。在回调函数中，所有的 HttpRequestMessage 字段都已经被初始化，除了 RequestUri 字段，
    /// 在回调函数执行后，如果没有提供值，它将被初始化。这里的字符串参数表示在构建 RequestUri 时应该使用的目标 URI 前缀。
    /// 基础实现会复制头部（headers），但会排除一些协议头部，比如 HTTP/2 伪头部（":authority"）。
    ///</summary>
    ///<param name="httpContext">传入请求</param>
    ///<param name="proxyRequest">传出的代理请求</param>
    ///<param name="destinationPrefix">所选目标服务器的uri前缀，可用于创建RequestUri</param>
    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        // 转发所有头部信息
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        // 自定义查询query 值
        var queryContext = new QueryTransformContext(httpContext.Request);
        queryContext.Collection.Remove("param1");
        queryContext.Collection["s"] = "xx2";
        // 分配自定义 URI。在此处连接时请注意额外的斜杠。RequestUtilities.MakeDestinationAddress 是一个安全的默认值。
        proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress("http://localhost:5047", httpContext.Request.Path, queryContext.QueryString);
        // 禁止原始请求标头，使用目标 Uri 中的标头
        proxyRequest.Headers.Host = null;
    }
}