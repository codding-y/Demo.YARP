var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()//���ReverseProxy��ط���DI
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));//�������ļ��м���ReverseProxy������


var app = builder.Build();

app.MapGet("/hello", () => "Hello World!");

app.MapReverseProxy();//ʹ��ReverseProxy�м��

app.Run();