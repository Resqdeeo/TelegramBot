using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Application.BotCommands;

public class ListCommand : IBotCommand
{
    public string Name => "/list";

    private readonly IOperationService _operationService;

    public ListCommand(IOperationService operationService)
    {
        _operationService = operationService;
    }
    
    public bool CanHandle(string messageText)
        => messageText.StartsWith(Name, StringComparison.OrdinalIgnoreCase);

    public bool CanHandle(string messageText, long userId) => CanHandle(messageText);

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var userId = message.Chat.Id;
        var ops = await _operationService.GetUserOperationsAsync(userId);

        if (ops.Count == 0)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "У вас пока нет операций.", cancellationToken: cancellationToken);
        }
        else
        {
            var msg = string.Join("\n", ops.Select(o => $"📝 {o.Title} — {o.ExecutionDateTime:g} ({o.Frequency})"));
            await botClient.SendTextMessageAsync(message.Chat.Id, msg, cancellationToken: cancellationToken);
        }
    }
}