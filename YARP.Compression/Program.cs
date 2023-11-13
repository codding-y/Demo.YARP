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
    AutomaticDecompression = DecompressionMethods.GZip, // ������Ӧѹ����ʽ
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
/// �Զ�������ת��
/// </summary>
class CustomTransformer : HttpTransformer
{
    ///<summary>
    /// A callback that is invoked prior to sending the proxied request. All HttpRequestMessage
    /// fields are initialized except RequestUri, which will be initialized after the
    /// callback if no value is provided. The string parameter represents the destination
    /// URI prefix that should be used when constructing the RequestUri. The headers
    /// are copied by the base implementation, excluding some protocol headers like HTTP/2
    /// pseudo headers (":authority").
    ///</summary>
    ///<param name="httpContext">��������</param>
    ///<param name="proxyRequest">�����Ĵ�������</param>
    ///<param name="destinationPrefix">��ѡĿ���������uriǰ׺�������ڴ���RequestUri</param>
    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        // ת������ͷ����Ϣ
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        // �Զ����ѯquery ֵ
        var queryContext = new QueryTransformContext(httpContext.Request);
        queryContext.Collection.Remove("param1");
        queryContext.Collection["s"] = "xx2";
        // �����Զ��� URI���ڴ˴�����ʱ��ע������б�ܡ�RequestUtilities.MakeDestinationAddress ��һ����ȫ��Ĭ��ֵ��
        proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress("http://localhost:5047", httpContext.Request.Path, queryContext.QueryString);
        // ��ֹԭʼ�����ͷ��ʹ��Ŀ�� Uri �еı�ͷ
        proxyRequest.Headers.Host = null;
    }
}