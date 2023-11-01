using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()//���ReverseProxy��ط���DI
    .LoadFromMemory(GetRoutes(), GetClusters());//���ڴ��м�������

var app = builder.Build();

// �Զ������ܵ������/ɾ��/�滻����
//app.MapReverseProxy(proxyPipeline =>
//{
//    // ʹ���Զ�������м��
//    proxyPipeline.Use(MyCustomProxyStep);
//    // �ڴ����Զ������ܵ�ʱ�������Ҫ�Ļ�������Ҫ���ǰ����������м��
//    proxyPipeline.UseSessionAffinity();
//    proxyPipeline.UseLoadBalancing();
//});
app.MapGet("/reload", (HttpContext httpContext) =>
{
    httpContext.RequestServices.GetRequiredService<InMemoryConfigProvider>().Update(GetRoutes(), GetClusters("https://www.baidu.com"));
});

app.MapReverseProxy();

app.Run();

static RouteConfig[] GetRoutes()
{
    return new[]
    {
        new RouteConfig()
        {
            RouteId = "route" + Random.Shared.Next(),
            ClusterId = "cluster1",
            Match = new RouteMatch
            {
                Path = "{**catch-all}"
            }
        }
    };
}
static ClusterConfig[] GetClusters(string? address = null)
{
    address ??= "https://cn.bing.com";
    //var debugMetadata = new Dictionary<string, string>
    //{
    //    { "debug", "true" }
    //};

    return new[]
    {
        new ClusterConfig()
        {
            ClusterId = "cluster1",
            //SessionAffinity = new SessionAffinityConfig { Enabled = true, Policy = "Cookie", AffinityKeyName = ".Yarp.ReverseProxy.Affinity" },
            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
            {
                { "destination1", new DestinationConfig() { Address = address } },
                //{ "debugdestination1", new DestinationConfig() {
                //    Address = "https://cn.bing.com",
                //    Metadata = debugMetadata  }
                //}
            }
        }
    };
}

/// <summary>
/// ������վ�����еı�ͷɸѡdestination���Զ��������
/// �鿴ÿ��Ŀ��Ԫ���ݣ�����������Ա�־����վ��ͷɸѡ��/��
/// </summary>
static Task MyCustomProxyStep(HttpContext context, Func<Task> next)
{
    // ����ͨ�������Ĵ������ж�ȡ����
    var useDebugDestinations = context.Request.Headers.TryGetValue("Debug", out var headerValues) && headerValues.Count == 1 && headerValues[0] == "true";

    // �����Ļ��洢ReverseProxyFeature���ù��ܱ����ض��ڴ�������ݣ���cluster��route��destination
    var availableDestinationsFeature = context.Features.Get<IReverseProxyFeature>();
    var filteredDestinations = new List<DestinationState>();

    // ��������ɸѡdestination
    foreach (var d in availableDestinationsFeature.AvailableDestinations)
    {
        //Todo: Replace with a lookup of metadata - but not currently exposed correctly here
        if (d.DestinationId.Contains("debug") == useDebugDestinations) { filteredDestinations.Add(d); }
    }
    availableDestinationsFeature.AvailableDestinations = filteredDestinations;

    return next();
}