using LeaderboardService.Models;
using LeaderboardService.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// 允许任何来源访问
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 注册自定义服务
builder.Services.AddSingleton<LeaderboardService.Services.LeaderboardService>();

// 注册控制器服务
builder.Services.AddControllers();

var app = builder.Build();

// 配置请求管道
app.UseRouting();

app.MapControllers();

app.Run();
