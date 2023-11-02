using YARP.Configuration.ConfigFilter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))//��������
                .AddConfigFilter<CustomConfigFilter>();//ע�����ù�����

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapReverseProxy();

app.Run();