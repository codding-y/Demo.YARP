using System.Text.RegularExpressions;
using Yarp.ReverseProxy.Configuration;

namespace YARP.Configuration.ConfigFilter;

public class CustomConfigFilter : IProxyConfigFilter
{
    // 用于匹配文本中的双花括号{{}}包围的单词（字母数字字符）
    private readonly Regex _exp = new("\\{\\{(\\w+)\\}\\}");

    // 集群的配置过滤器，将依次传递给每个集群，它应该按原样返回，或者 克隆并创建具有更新更改的新版本
    // 此示例查看目标地址（destination addresses），任何形式{{key}}都将被匹配到，并用此key获取环境变量里对应的value
    // 作为环境变量。当托管在Azure等中时，这很有用，因为它能够以简单的方式替换
    public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig origCluster, CancellationToken cancel)
    {
        // 每个集群都有一个 destination 字典，它是只读的，所以我们将用更新创建一个 destination 字典
        var newDests = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase);

        foreach (var d in origCluster.Destinations)
        {
            var origAddress = d.Value.Address;
            if (_exp.IsMatch(origAddress))
            {
                // 用先前定义的正则表达式_exp来匹配字符串，然后提取第一个匹配的结果的捕获组（Group）中索引为1的值: baidu。
                var lookup = _exp.Matches(origAddress)[0].Groups[1].Value;
                // 根据key（baidu）:获取 value (https://www.baidu.com)
                var newAddress = Environment.GetEnvironmentVariable(lookup);

                if (string.IsNullOrWhiteSpace(newAddress))
                {
                    throw new ArgumentException($"Configuration Filter Error: Substitution for '{lookup}' in cluster '{d.Key}' not found as an environment variable.");
                }

                // c# 9 "with" 语法： 克隆并初始化 record
                var modifiedDest = d.Value with { Address = newAddress };
                newDests.Add(d.Key, modifiedDest);
            }
            else
            {
                newDests.Add(d.Key, d.Value);
            }
        }

        return new ValueTask<ClusterConfig>(origCluster with { Destinations = newDests });
    }

    public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig? cluster, CancellationToken cancel)
    {
        // Example: 不要让基于配置的路由优先于基于代码的路由。
        // 数字越低，优先级越高。代码路由默认为0。
        if (route.Order.HasValue && route.Order.Value < 1)
        {
            return new ValueTask<RouteConfig>(route with { Order = 1 });
        }

        return new ValueTask<RouteConfig>(route);
    }
}
