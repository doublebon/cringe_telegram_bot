using Microsoft.Extensions.Configuration;

namespace Cringe.Config;

public interface ICringeConfig
{ 
    string GetBotToken(); 
    IConfiguration GetConfiguration();
    IEnumerable<string> GetAnswers();
}