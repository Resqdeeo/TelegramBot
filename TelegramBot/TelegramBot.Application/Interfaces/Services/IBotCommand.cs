using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Application.Interfaces.Services;

public interface IBotCommand
{
    string Name { get; }
    Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
    bool CanHandle(string messageText);
    
    bool CanHandle(string messageText, long userId);
}