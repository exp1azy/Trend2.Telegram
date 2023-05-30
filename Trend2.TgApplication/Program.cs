using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Threading;
using System.IO;
using TL;
using Trend2.Telegram;
using Trend2.Telegram.EventArgs;
using Trend2.TgApplication.Data;
using Trend2.TgApplication.Hubs;
using Trend2.TgApplication.Services;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddTransient<SourceService>();
builder.Services.AddTransient<ArticleService>();

var config = builder.Configuration;
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(config["ConnectionString"]));

var tgCollector = new TgCollector(config);
var consoleHub = new ConsoleHub();

var ev = new AsyncAutoResetEvent();

tgCollector.ChannelListStarted += (s, ea) => 
{  
    consoleHub.SendMessageAsync("����� ������� ������ �������...");
};

tgCollector.ChannelListCompleted += (s, ea) =>
{   
    consoleHub.SendMessageAsync("������� ������ ������� ��������");
};

tgCollector.PostsBatchRead += (s, ea) =>
{  
    consoleHub.SendMessageAsync($" >>> {ea.Count} ����� ���������");
};

tgCollector.ChannelStarted += (s, ea) =>
{
    consoleHub.SendMessageAsync($"\n������ ��������� ������ <{ea.Channel.Title}>(@{ea.Channel.Site} | ID: {ea.Channel.Id})");
};

tgCollector.ChannelSuccess += (s, ea) =>
{
    consoleHub.SendMessageAsync($"����� <{ea.Channel.Title}>(@{ea.Channel.Site} | ID: {ea.Channel.Id}) ���������.\n" +
    $"��������� {ea.Posts} ���������\n" +
    $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
};

tgCollector.ChannelError += (s, ea) =>
{  
    consoleHub.SendMessageAsync($"������ � �������� ��������� ������ <{ea.Channel.Title}>(@{ea.Channel.Site} | {ea.Channel.Id}):\n{ea.Exception}");

    if (ea.Exception != null)
    {
        WriteErrorLogs(ea.Exception);
    }  
};

tgCollector.Started += (s, ea) =>
{  
    consoleHub.SendMessageAsync("������� ������� ��������������� �����");
};

tgCollector.Stopped += (s, ea) =>
{
    if (ea.Exception != null)
    {
        consoleHub.SendMessageAsync($"������� ������� �� ������� ������:\n{ea.Exception}");

        WriteErrorLogs(ea.Exception);
    }
    else
    {
        consoleHub.SendMessageAsync("������� ����� ������ ����������\n");
    }
    ev.Set();
};

tgCollector.LoginError += (s, ea) =>
{
    consoleHub.SendMessageAsync($"������ �����������: {ea.Message ?? "null"}{(ea is RpcException rpce ? $" - {rpce.X}" : string.Empty)}");
};

tgCollector.VerificationCodeRequested += (s, ea) =>
{
    consoleHub.SendMessageAsync("��������� ��� �������������");
};

builder.Services.AddSingleton(consoleHub);
builder.Services.AddSingleton(tgCollector);

builder.Services.AddSignalR();

builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Source/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomePage}/{id?}");

app.MapHub<ConsoleHub>("/console");
app.Run();

async Task WriteErrorLogs(Exception ex)
{
    using (StreamWriter writer = new StreamWriter("ExceptionLog.txt", false))
    {
        await writer.WriteLineAsync(ex.ToString());
    }
}