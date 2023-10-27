var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()//添加ReverseProxy相关服务到DI
                                  //从配置文件中加载ReverseProxy的设置
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/hello", () => "Hello World!");

app.MapReverseProxy();//使用ReverseProxy中间件

app.Run();