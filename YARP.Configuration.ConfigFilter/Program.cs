using YARP.Configuration.ConfigFilter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))//¼ÓÔØÅäÖÃ
                .AddConfigFilter<CustomConfigFilter>();//×¢²áÅäÖÃ¹ıÂËÆ÷

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapReverseProxy();

app.Run();