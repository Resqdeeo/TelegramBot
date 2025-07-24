using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Application.Interfaces.Services;

public interface IBotCallbackCommand
{
    Task ExecuteAsync(ITelegramBotClient botClient, CallbackQuery callback, CancellationToken cancellationToken);
}