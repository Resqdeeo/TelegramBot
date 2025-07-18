using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Application.BotCommands;

public class HelpCommand : IBotCommand
{
    public string Name => "/help";
    
    public bool CanHandle(string messageText) => messageText.StartsWith(Name);
    
    public bool CanHandle(string messageText, long userId) => CanHandle(messageText);

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        string helpText = "Вот список доступных команд:\n\n" +
                          "/help - Получить список команд\n" +
                          "/add - Добавить новую операцию (ввод по шагам)\n" +
                          "/delete - Удалить существующую операцию (выбор из списка)";

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: helpText,
            cancellationToken: cancellationToken
        );
    }
}