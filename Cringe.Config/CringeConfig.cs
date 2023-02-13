using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cringe.Config;

public class CringeConfig : ICringeConfig
{
    private readonly IConfiguration _config;

    public CringeConfig()
    {
        _config = PrepareConfiguration();
    }

    private IConfiguration PrepareConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
    
    public IConfiguration GetConfiguration()
    {
        return _config;
    }
    
    public string GetBotToken()
    {
        return _config.GetValue<string>("TelegramBotToken") ??
               throw new KeyNotFoundException("Can't find 'TelegramBotToken' key at config file");
    }
    
    public IEnumerable<string> GetAnswers()
    {
        return _config.GetSection("Answers").Get<string[]>() ??
               throw new KeyNotFoundException("Can't find 'TelegramBotToken' key at config file");
    }
}