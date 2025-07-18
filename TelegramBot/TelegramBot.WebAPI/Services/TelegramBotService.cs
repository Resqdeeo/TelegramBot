using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.WebAPI.Services;

public class TelegramBotService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;
    private ITelegramBotClient _botClient;

    public TelegramBotService(IConfiguration configuration, IServiceProvider services)
    {
        _configuration = configuration;
        _services = services;
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
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
            return;

        var message = update.Message;
        var userId = message.Chat.Id;

        using var scope = _services.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IBotCommand>();

        var handler = handlers.FirstOrDefault(h => h.CanHandle(message.Text, userId));

        if (handler != null)
            await handler.ExecuteAsync(botClient, message, cancellationToken);
        else
            await botClient.SendTextMessageAsync(message.Chat.Id, "Неизвестная команда. Напиши /help", cancellationToken: cancellationToken);
    }
    
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}