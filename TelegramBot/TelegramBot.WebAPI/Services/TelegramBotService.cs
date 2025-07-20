using Quartz;
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
    private readonly ISchedulerFactory _schedulerFactory;
    private ITelegramBotClient _botClient;

    public TelegramBotService(IConfiguration configuration, IServiceProvider services, ISchedulerFactory schedulerFactory)
    {
        _configuration = configuration;
        _services = services;
        _schedulerFactory = schedulerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("TELEGRAM_TOKEN environment variable is not set.");

        _botClient = new TelegramBotClient(token);
        await InitializeScheduler();

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
    }

    private async Task InitializeScheduler()
    {
        using var scope = _services.CreateScope();
        var scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.Start();
        
        // Восстановление задач из базы
        var messageScheduler = scope.ServiceProvider.GetRequiredService<IMessageScheduler>();
        await messageScheduler.StartAsync();
    }
    
    
    
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            using var scopeCallback = _services.CreateScope();
            var callbackHandlers = scopeCallback.ServiceProvider.GetServices<IBotCommand>();
            var callback = update.CallbackQuery;

            var callbackHandler = callbackHandlers.FirstOrDefault(h => h.CanHandle(callback.Data, callback.Message.Chat.Id));

            if (callbackHandler is IBotCallbackCommand botCallbackCommand && callback != null)
            {
                await botCallbackCommand.ExecuteAsync(botClient, callback, cancellationToken);
            }
            return;
        }

        if(update.Type != UpdateType.Message || update.Message?.Text == null )
        {
            return;
        }

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
    
    private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        await Task.Delay(1000, cancellationToken);
    }
}