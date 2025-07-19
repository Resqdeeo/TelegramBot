using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Application.Abstractions;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Application.Utlis;
using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.BotCommands;

public class EditOperationCommand : IBotCommand, IBotCallbackCommand
{
    public string Name { get; } = "/edit";

    private readonly IUserStateService _stateService;
    private readonly IOperationService _operationService;

    public EditOperationCommand(IUserStateService stateService, IOperationService operationService)
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
            if(context.EditStep == EditOperationStep.None)
            {
                context.EditStep = EditOperationStep.EditChoice;

                var inlineKeyboard = InlineUtils.GetInlineKeyboard(ops
                    .ToDictionary(o => o.Id, o => $"📝 {o.Title} — {o.ExecutionDateTime:g} ({o.Frequency})"), Name);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Выберите оперцаию для редактирования:",
                    replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                return;
            }
            var operation = await _operationService.GetOperationByIdAsync(userId, context.Id);

            switch (context.EditStep)
            {
                case EditOperationStep.EditTitle:
                    operation.Title = message.Text;
                    await _operationService.UpdateOperationAsync(userId, operation);
                    _stateService.ClearContext(userId);
                    break;
                case EditOperationStep.EditDescription:
                    operation.Description = message.Text;
                    await _operationService.UpdateOperationAsync(userId, operation);
                    _stateService.ClearContext(userId);
                    break;
                case EditOperationStep.EditDate:
                    var format = "dd.MM.yyyy HH:mm";
                    if (DateTime.TryParseExact(message.Text, format, System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var date))
                    {
                        date = DateTime.SpecifyKind(date, DateTimeKind.Local);
                        var utcDateTime = date.ToUniversalTime();
                        operation.ExecutionDateTime = utcDateTime;
                        await _operationService.UpdateOperationAsync(userId, operation);
                        _stateService.ClearContext(userId);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(userId, "Неверный формат даты. Повторите попытку (дд.мм.гггг чч:мм)", cancellationToken: cancellationToken);
                        return;
                    }
                    break;
                case EditOperationStep.EditTime:
                    if (int.TryParse(message.Text, out int freqIndex) && Enum.IsDefined(typeof(OperationFrequency), freqIndex))
                    {
                        operation.Frequency = (OperationFrequency)freqIndex;

                        await _operationService.UpdateOperationAsync(userId, operation);
                        _stateService.ClearContext(userId);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(userId, "Неверный выбор. Введите номер из списка.", cancellationToken: cancellationToken);
                        return;
                    }
                    break;
            }
            await botClient.SendTextMessageAsync(userId, "Операция успешно изменена ✅", cancellationToken: cancellationToken);
        }
    }

    public bool CanHandle(string messageText) => messageText.StartsWith(Name);

    public bool CanHandle(string messageText, long userId)
    {
        var context = _stateService.GetContext(userId);

        return context.EditStep != EditOperationStep.None || messageText.StartsWith(Name);
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, CallbackQuery callback,
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId,
            cancellationToken: cancellationToken);
        var message = callback.Message!;
        var userId = message.Chat.Id;
        var context = _stateService.GetContext(userId);
        var texts = callback.Data!.Split(":");
        var id = texts[1];
        var choice = texts.Last();
        
        context.Id = long.Parse(id);

        if (choice.Equals("cancel"))
        {
            _stateService.ClearContext(userId);
            return;
        }


        var operation = await _operationService.GetOperationByIdAsync(userId, long.Parse(id));

        switch (choice)
        {
            case "title":
                context.EditStep = EditOperationStep.EditTitle;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите новое название",
                    replyMarkup: InlineUtils.GetInlineKeyboardBackButton(Name, id),
                    cancellationToken: cancellationToken);
                break;
            case "description":
                context.EditStep = EditOperationStep.EditDescription;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите новое описание",
                    replyMarkup: InlineUtils.GetInlineKeyboardBackButton(Name, id),
                    cancellationToken: cancellationToken);
                break;
            case "date":
                context.EditStep = EditOperationStep.EditDate;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите новую дату",
                    replyMarkup: InlineUtils.GetInlineKeyboardBackButton(Name, id),
                    cancellationToken: cancellationToken);
                break;
            case "time":
                context.EditStep = EditOperationStep.EditTime;
                string freqList = string.Join("\n",
                    Enum.GetNames(typeof(OperationFrequency)).Select((f, i) => $"{i}. {f}"));
                await botClient.SendTextMessageAsync(userId,
                    $"Выберите периодичность операции (введите номер):\n{freqList}",
                    replyMarkup: InlineUtils.GetInlineKeyboardBackButton(Name, id),
                    cancellationToken: cancellationToken);

                break;
            case "back":
                context.EditStep = EditOperationStep.EditChoice;
                break;
        }

        if (context.EditStep == EditOperationStep.EditChoice)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                [
                    [
                        InlineKeyboardButton.WithCallbackData("Название", $"{Name}:{id}:title"),
                        InlineKeyboardButton.WithCallbackData("Описание", $"{Name}:{id}:description")
                    ],
                    [
                        InlineKeyboardButton.WithCallbackData("Дата", $"{Name}:{id}:date"),
                        InlineKeyboardButton.WithCallbackData("Периодичность операции", $"{Name}:{id}:time")
                    ],
                    [
                        InlineKeyboardButton.WithCallbackData("Отмена", $"{Name}:{id}:cancel"),
                    ]
                ]
            );

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text:  $"📝Название: {operation.Title}\nОписание: {operation.Title}\nДата: {operation.ExecutionDateTime:g}\nПереодичность: ({operation.Frequency})",
                replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        }
    }
}