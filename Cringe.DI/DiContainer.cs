using Cringe.Config;
using Cringe.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;

namespace Cringe.DI;

public static class DiContainer
{
    public static IHost RegisterServices()
    {
        var diContainer = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ICringeConfig,  CringeConfig>();
                services.AddSingleton<IStartup, Startup>();
                services.AddSingleton<ITelegramBotClient>(sp =>
                {
                    var botToken = sp.GetRequiredService<ICringeConfig>().GetBotToken();
                    return new TelegramBotClient(botToken);
                });
                
            })
            .UseSerilog()
            .Build();

        var logConfiguration = diContainer.Services.GetRequiredService<ICringeConfig>();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(logConfiguration.GetConfiguration())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        return diContainer;
    }
}