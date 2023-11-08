using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using YARP.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                .AddTransforms(transformBuilderContext =>  // Add transforms inline
                {
                    // 仅对需要身份验证的路由执行此操作。
                    if (string.Equals("myPolicy", transformBuilderContext.Route.AuthorizationPolicy))
                    {
                        transformBuilderContext.AddRequestTransform(async transformContext =>
                        {
                            // 在请求路由之后，AuthN和AuthZ将已经完成。
                            var ticket = await transformContext.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            var tokenService = transformContext.HttpContext.RequestServices.GetRequiredService<TokenService>();
                            var token = await tokenService.GetAuthTokenAsync(ticket.Principal);

                            // 拒绝无效请求
                            if (string.IsNullOrEmpty(token))
                            {
                                var response = transformContext.HttpContext.Response;
                                response.StatusCode = 401;
                                return;
                            }

                            transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        });
                    }
                });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

builder.Services.AddAuthorization(options =>
{
    // 创建一个名为 myPolicy 的策略，代理可以使用这个策略，在配置文件中指定 策略名称。
    // 该策略会校验已登录 用户的 声明
    options.AddPolicy("myPolicy", builder => builder
        .RequireClaim("myCustomClaim", "green")
        .RequireAuthenticatedUser());

    // 默认策略是要求身份验证，但不要求其他声明
    // 取消注释以下内容将无效
    // 选项。DefaultPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    // FallbackPolicy用于未在配置中指定策略的路由
    // 将所有未指定策略的路由设为匿名（这是默认设置）。
    options.FallbackPolicy = null;
    // 或者使所有未指定策略的路由都需要一些身份验证：
    // options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();            
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapReverseProxy();

app.Run();
