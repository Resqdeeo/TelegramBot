using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Application.Abstractions;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Application.Utlis;

namespace TelegramBot.Application.BotCommands;

public class DeleteOperationCommand : IBotCommand, IBotCallbackCommand
{
    public string Name => "/delete";

    private readonly IUserStateService _stateService;
    private readonly IOperationService _operationService;
    
    public DeleteOperationCommand(IUserStateService stateService, IOperationService operationService)
    {
        _stateService = stateService;
        _operationService = operationService;
    }
    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken: cancellationToken);
        var userId = message.Chat.Id;
        var context = _stateService.GetContext(userId);

        var ops = await _operationService.GetUserOperationsAsync(userId);

        if (ops.Count == 0)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "У вас пока нет операций.",
                cancellationToken: cancellationToken);
        }
        else
        {

            context.DeleteStep = DeleteOperationStep.DeleteSelect;

            var inlineKeyboard = InlineUtils.GetInlineKeyboard(ops
                .ToDictionary(o => o.Id, o => $"📝 {o.Title} — {o.ExecutionDateTime:g} ({o.Frequency})"), Name);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Выберите оперцаию для удаления:",
                replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        }
    }

    public bool CanHandle(string messageText) => messageText.StartsWith(Name);

    public bool CanHandle(string messageText, long userId)
    {
        return messageText.StartsWith(Name);
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, CallbackQuery callback,
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId, cancellationToken: cancellationToken);
        var message = callback.Message!;
        var userId = message.Chat.Id;
        var context = _stateService.GetContext(userId);

        var texts = callback.Data!.Split(":");
        var id = texts[1];
        var choice = texts.Last();

        switch (context.DeleteStep)
        {
            case DeleteOperationStep.DeleteSelect:
                context.DeleteStep = DeleteOperationStep.DeleteChoice;
                await botClient.SendTextMessageAsync(userId, "Удалить операцию:",
                    replyMarkup: InlineUtils.GetYesNoKeyboard(Name, id), cancellationToken: cancellationToken);
                break;
            case DeleteOperationStep.DeleteChoice:
                if (choice.Equals("yes"))
                {
                    await _operationService.DeleteOperationAsync(userId, long.Parse(id));
                    await botClient.SendTextMessageAsync(userId, "Операция успешно удалена",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(userId, "Удаление отменено",
                        cancellationToken: cancellationToken);
                }

                _stateService.ClearContext(userId);
                break;
        }
    }
}