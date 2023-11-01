var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()//添加ReverseProxy相关服务到DI
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));//从配置文件中加载ReverseProxy的设置


var app = builder.Build();

app.MapGet("/hello", () => "Hello World!");

app.MapReverseProxy();//使用ReverseProxy中间件

app.Run();