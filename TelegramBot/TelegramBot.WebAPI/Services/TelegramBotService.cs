using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.WebAPI.Services;

public class TelegramBotService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private ITelegramBotClient _botClient;

    public TelegramBotService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("TELEGRAM_TOKEN environment variable is not set.");

        _botClient = new TelegramBotClient(token);
        
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            stoppingToken
        );
        
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text == "/start")
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Hello, World!",
                cancellationToken: cancellationToken
            );
        }
    }
    
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}