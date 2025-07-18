using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Application.BotCommands;

public class StartCommand : IBotCommand
{
    public string Name => "/start";
    
    public bool CanHandle(string messageText)
        => messageText.StartsWith(Name, StringComparison.OrdinalIgnoreCase);
    
    public bool CanHandle(string messageText, long userId) => CanHandle(messageText);

    private readonly IUserService _userService;
    
    public StartCommand(IUserService userService)
    {
        _userService = userService;
    }
    
    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await _userService.CreateUserIfNotExistsAsync(message.Chat.Id, message.Chat.Username ?? "Unknown");
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Привет! 👋 Я бот для управления операциями.\n\nНапиши /help, чтобы посмотреть, что я умею.",
            cancellationToken: cancellationToken
        );
    }
}