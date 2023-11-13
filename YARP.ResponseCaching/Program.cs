var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy()//���ReverseProxy��ط���DI
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));//�������ļ��м���ReverseProxy������

builder.Services.AddResponseCaching(options =>
{
    options.UseCaseSensitivePaths = false;
    options.SizeLimit = options.SizeLimit * 10;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ʹ�� CORS �м��ʱ�������� UseResponseCaching ֮ǰ���� UseCors��
// app.UseCors();
app.UseRouting();

app.Use(async (context, next) =>
{
    var header = context.Request.Headers;
    var cacheControl = header.CacheControl;
    if (!string.IsNullOrEmpty(header.CacheControl))
    {
        header.CacheControl = new Microsoft.Extensions.Primitives.StringValues("max-age");
    }

    await next(context);
});
app.UseResponseCaching();
app.Use(async (context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl =
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
        {
            Public = true,
            MaxAge = TimeSpan.FromSeconds(10)
        };

    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new string[] { "Accept-Encoding" };

    await next(context);
});
app.MapReverseProxy();

app.MapGet("/", () => DateTime.Now.ToLongTimeString());

app.Run();