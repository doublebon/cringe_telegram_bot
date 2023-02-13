using Cringe.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace Cringe.Core;

public class Startup : IStartup
{
    private readonly ILogger<Startup> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly CancellationToken _cancellationToken;
    private readonly string[] _answers;

    public Startup(ILogger<Startup> logger, ITelegramBotClient botClient, ICringeConfig config)
    {
        _logger = logger;
        _botClient = botClient;
        _cancellationToken = new CancellationTokenSource().Token;
        _answers = config.GetAnswers().ToArray();
    }

    /**
     * Ignore all messages, that were sent by users, while the bot was turned off
     */
    private async Task<ReceiverOptions> ConfigureReceiverOptions()
    {
        var lastChatMessageId = await GetLastUserMessageIdAsync();
        return new ReceiverOptions
        {
            Offset = lastChatMessageId != 0 ? lastChatMessageId + 1 : null, // ignore old messages
            AllowedUpdates = new[]{ UpdateType.InlineQuery }
        };
    }
    
    private async Task<int> GetLastUserMessageIdAsync()
    {
        try
        {
            var clientLastUpdates = await _botClient.GetUpdatesAsync(cancellationToken: _cancellationToken);
            _logger.LogInformation("[STARTUP] Found client last updates count: {lastUpdatesCount}", clientLastUpdates.Length);
            return clientLastUpdates.Length > 0 ? clientLastUpdates[clientLastUpdates.Length - 1].Id : 0;
        }
        catch (Exception e)
        {
            _logger.LogError("[STARTUP] Can't get any bot updates. Possibly invalid token.\n{errorMessage}", e);
            Environment.Exit(1);
        }
        return 0;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[HANDLE_UPDATE_INFO] {responseBody}",Newtonsoft.Json.JsonConvert.SerializeObject(update));
        switch (update.Type)
        {
            case UpdateType.InlineQuery:
                {
                    var rand = new Random(Guid.NewGuid().GetHashCode());
                    var answer = _answers[rand.Next(_answers.Length)];
                    var inlineBody = update.InlineQuery;
                    await botClient.AnswerInlineQueryAsync(inlineBody!.Id, new InlineQueryResult[]
                        {
                            new InlineQueryResultArticle(
                                id: Guid.NewGuid().ToString(),
                                title:"Кринж или база?", 
                                inputMessageContent: new InputTextMessageContent($"{answer}")
                                {
                                    ParseMode = ParseMode.Html
                                }){ 
                                ThumbUrl = "https://cdn.eslgaming.com/play/eslgfx/gfx/logos/playerphotos/15149000/15149686_medium.jpg",
                                Description = "Твой собеседник глаголит истину или скрывается за маской клоуна?",
                            }
                        }, isPersonal: false, cacheTime: 0, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                break;
        }
    }
    
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}", 
            RequestException requestException
                => $"Telegram Request Error:\n[{requestException.HttpStatusCode}]\n{requestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError("[POLLING_ERROR] HandlePollingError has some error msg:\n{errorMessage}", errorMessage);
        return Task.FromResult(false);
    }
    
    public async void StartReceiving()
    {
        var reserveOptions = await ConfigureReceiverOptions();
        _logger.LogInformation("[STARTUP] Bot was started. Start receiving...");
        _botClient.StartReceiving(
                    HandleUpdateAsync,
                    HandlePollingErrorAsync,
                    reserveOptions,
                    _cancellationToken
                );
    }
}