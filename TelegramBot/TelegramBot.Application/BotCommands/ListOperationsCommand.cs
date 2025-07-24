using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Application.Contracts;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Application.BotCommands;

public class ListOperationsCommand : IBotCommand, IBotCallbackCommand
{
    public string Name => "/list";

    private readonly IOperationService _operationService;
    private readonly IOperationHistoryService _historyService;

    public ListOperationsCommand(
        IOperationService operationService,
        IOperationHistoryService historyService)
    {
        _operationService = operationService;
        _historyService = historyService;
    }

    public bool CanHandle(string messageText)
        => messageText.StartsWith(Name, StringComparison.OrdinalIgnoreCase);

    public bool CanHandle(string messageText, long userId)
        => CanHandle(messageText);

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);

        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData("На день", $"{Name}:day"),
                    InlineKeyboardButton.WithCallbackData("На неделю", $"{Name}:week")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("На месяц", $"{Name}:month"),
                    InlineKeyboardButton.WithCallbackData("Все операции", $"{Name}:all")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("История операций", $"{Name}:history")
                ]
            ]
        );

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Выберите период для отображения операций:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, CallbackQuery callback,
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId, cancellationToken);

        var userId = callback.Message.Chat.Id;
        var period = callback.Data.Split(':').Last();

        if (period == "history")
        {
            var historyItems = await _historyService.GetOperationHistoryAsync(userId);
            await SendHistoryList(botClient, userId, historyItems, cancellationToken);
            return;
        }

        DateTime from = DateTime.UtcNow;
        DateTime to = from;
        string periodName = "";

        switch (period)
        {
            case "day":
                to = from.AddDays(1);
                periodName = "на день";
                break;
            case "week":
                to = from.AddDays(7);
                periodName = "на неделю";
                break;
            case "month":
                to = from.AddMonths(1);
                periodName = "на месяц";
                break;
            case "all":
                var allOps = await _operationService.GetUserOperationsAsync(userId);
                var futureOps = allOps
                    .Where(o => o.ExecutionDateTime >= DateTime.UtcNow).ToList();
                await SendOperationsList(botClient, userId, futureOps, "Все операции", cancellationToken);
                return;
        }

        var ops = await _operationService.GetUpcomingOperationsAsync(userId, from, to);
        var futOps = ops.Where(o => o.ExecutionDateTime >= DateTime.UtcNow).ToList();
        await SendOperationsList(botClient, userId, futOps, $"Операции {periodName}", cancellationToken);
    }

    private async Task SendOperationsList(ITelegramBotClient botClient, long chatId, List<OperationDto> ops,
        string title, CancellationToken cancellationToken)
    {
        if (ops.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                $"У вас нет операций за этот период",
                cancellationToken: cancellationToken);
        }
        else
        {
            var msg = $"{title}:\n\n" + string.Join("\n\n", ops.Select(o =>
                $"❗️ {o.Title}\n" +
                $"⏱️ {o.ExecutionDateTime:g} ({o.Frequency})\n" +
                $"📝 {(string.IsNullOrEmpty(o.Description) ? "Без описания" : o.Description)}"));

            await botClient.SendTextMessageAsync(
                chatId,
                msg,
                cancellationToken: cancellationToken);
        }
    }
    
    private async Task SendHistoryList(
        ITelegramBotClient botClient, 
        long chatId, 
        List<OperationHistoryDto> historyItems,
        CancellationToken cancellationToken)
    {
        if (historyItems.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "У вас нет ранее выполненных операций",
                cancellationToken: cancellationToken);
        }
        else
        {
            var msg = "История выполненных операций:\n\n" + 
                      string.Join("\n\n", historyItems.Select(h => 
                          $"✅ {h.OperationTitle}\n" +
                          $"⏱️ Выполнено: {h.PerformedAt:g}\n" +
                          $"📝 {(string.IsNullOrEmpty(h.OperationDescription) ? "Без описания" : h.OperationDescription)}"));

            await botClient.SendTextMessageAsync(
                chatId,
                msg,
                cancellationToken: cancellationToken);
        }
    }
}