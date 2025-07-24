using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Application.BotCommands;

public class StartCommand : IBotCommand
{
    public string Name => "/start";
    
    private readonly IUserService _userService;
    
    public StartCommand(IUserService userService)
    {
        _userService = userService;
    }

    public bool CanHandle(string messageText)
        => messageText.StartsWith(Name, StringComparison.OrdinalIgnoreCase);
    
    public bool CanHandle(string messageText, long userId) => CanHandle(messageText);

    
    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await _userService.CreateUserIfNotExistsAsync(message.Chat.Id, message.Chat.Username ?? "Unknown");
        
        var replyKeyboardMarkup = new ReplyKeyboardMarkup([
            [
                new KeyboardButton("/add"),
                new KeyboardButton("/list")
            ],
            [
                new KeyboardButton("/edit"),
                new KeyboardButton("/delete")
            ],
            [
                new KeyboardButton("/help")
            ]
        ])
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Привет! 👋 Я бот для управления операциями.\n\nНапиши /help, чтобы посмотреть, что я умею.",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken
        );
    }
}