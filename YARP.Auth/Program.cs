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
                    // ������Ҫ�����֤��·��ִ�д˲�����
                    if (string.Equals("myPolicy", transformBuilderContext.Route.AuthorizationPolicy))
                    {
                        transformBuilderContext.AddRequestTransform(async transformContext =>
                        {
                            // ������·��֮��AuthN��AuthZ���Ѿ���ɡ�
                            var ticket = await transformContext.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            var tokenService = transformContext.HttpContext.RequestServices.GetRequiredService<TokenService>();
                            var token = await tokenService.GetAuthTokenAsync(ticket.Principal);

                            // �ܾ���Ч����
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
    // ����һ����Ϊ myPolicy �Ĳ��ԣ��������ʹ��������ԣ��������ļ���ָ�� �������ơ�
    // �ò��Ի�У���ѵ�¼ �û��� ����
    options.AddPolicy("myPolicy", builder => builder
        .RequireClaim("myCustomClaim", "green")
        .RequireAuthenticatedUser());

    // Ĭ�ϲ�����Ҫ�������֤������Ҫ����������
    // ȡ��ע���������ݽ���Ч
    // ѡ�DefaultPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    // FallbackPolicy����δ��������ָ�����Ե�·��
    // ������δָ�����Ե�·����Ϊ����������Ĭ�����ã���
    options.FallbackPolicy = null;
    // ����ʹ����δָ�����Ե�·�ɶ���ҪһЩ�����֤��
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
