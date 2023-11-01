using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()//添加ReverseProxy相关服务到DI
    .LoadFromMemory(GetRoutes(), GetClusters());//从内存中加载配置

var app = builder.Build();

// 自定义代理管道和添加/删除/替换步骤
//app.MapReverseProxy(proxyPipeline =>
//{
//    // 使用自定义代理中间件
//    proxyPipeline.Use(MyCustomProxyStep);
//    // 在创建自定义代理管道时（如果需要的话），不要忘记包含这两个中间件
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
/// 根据入站请求中的标头筛选destination的自定义代理步骤
/// 查看每个目标元数据，并根据其调试标志和入站标头筛选入/出
/// </summary>
static Task MyCustomProxyStep(HttpContext context, Func<Task> next)
{
    // 可以通过上下文从请求中读取数据
    var useDebugDestinations = context.Request.Headers.TryGetValue("Debug", out var headerValues) && headerValues.Count == 1 && headerValues[0] == "true";

    // 上下文还存储ReverseProxyFeature，该功能保存特定于代理的数据，如cluster、route和destination
    var availableDestinationsFeature = context.Features.Get<IReverseProxyFeature>();
    var filteredDestinations = new List<DestinationState>();

    // 根据条件筛选destination
    foreach (var d in availableDestinationsFeature.AvailableDestinations)
    {
        //Todo: Replace with a lookup of metadata - but not currently exposed correctly here
        if (d.DestinationId.Contains("debug") == useDebugDestinations) { filteredDestinations.Add(d); }
    }
    availableDestinationsFeature.AvailableDestinations = filteredDestinations;

    return next();
}