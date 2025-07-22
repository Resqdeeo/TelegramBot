using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Application.Abstractions;
using TelegramBot.Application.Contracts;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.BotCommands;

public class AddOperationCommand : IBotCommand
{
    public string Name => "/add";
    
    private readonly IUserStateService _stateService;
    private readonly IOperationService _operationService;

    public AddOperationCommand(IUserStateService stateService, IOperationService operationService)
    {
        _stateService = stateService;
        _operationService = operationService;
    }
    
    public bool CanHandle(string messageText) => messageText.StartsWith("/add");
    
    public bool CanHandle(string messageText, long userId)
    {
        var context = _stateService.GetContext(userId);
        return context.Step != AddOperationStep.None || messageText.StartsWith("/add");
    }
    
    public async Task ExecuteAsync(ITelegramBotClient client, Message message, CancellationToken cancellationToken)
    {
        long userId = message.Chat.Id;
        var context = _stateService.GetContext(userId);
        var text = message.Text?.Trim();

        if (context.Step == AddOperationStep.None)
        {
            context.Step = AddOperationStep.AwaitingTitle;
            await client.SendTextMessageAsync(userId, "Введите название операции:", 
                cancellationToken: cancellationToken);
            return;
        }

        switch (context.Step)
        {
            case AddOperationStep.AwaitingTitle:
                context.Title = text;
                context.Step = AddOperationStep.AwaitingDescription;
                await client.SendTextMessageAsync(userId, "Введите описание:", 
                    cancellationToken: cancellationToken);
                break;

            case AddOperationStep.AwaitingDescription:
                context.Description = text;
                context.Step = AddOperationStep.AwaitingDateTime;
                await client.SendTextMessageAsync(userId, 
                    "Введите дату и время выполнения (в формате дд.мм.гггг чч:мм):",
                    cancellationToken: cancellationToken);
                break;

            case AddOperationStep.AwaitingDateTime:
                var format = "dd.MM.yyyy HH:mm";
                if (DateTime.TryParseExact(text, format, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var date))
                {
                    date = DateTime.SpecifyKind(date, DateTimeKind.Local);
                    var utcDateTime = date.ToUniversalTime();
                    if (utcDateTime < DateTime.UtcNow)
                    {
                        await client.SendTextMessageAsync(userId, 
                            "Нельзя добавлять операции с датой в прошлом. Введите корректную дату",
                            cancellationToken: cancellationToken);
                        return;
                    }
                    context.ExecutionDateTime = utcDateTime;
                    context.Step = AddOperationStep.AwaitingFrequency;

                    string freqList = string.Join("\n", Enum.GetNames(typeof(OperationFrequency)).Select((f, i) => $"{i}. {f}"));
                    await client.SendTextMessageAsync(userId, 
                        $"Выберите периодичность операции (введите номер):\n{freqList}", 
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await client.SendTextMessageAsync(userId, 
                        "Неверный формат даты. Повторите попытку (дд.мм.гггг чч:мм)", 
                        cancellationToken: cancellationToken);
                }
                break;

            case AddOperationStep.AwaitingFrequency:
                if (int.TryParse(text, out int freqIndex) && Enum.IsDefined(typeof(OperationFrequency), freqIndex))
                {
                    context.Frequency = (OperationFrequency)freqIndex;

                    await _operationService.AddOperationAsync(userId, new OperationDto
                    {
                        Title = context.Title!,
                        Description = context.Description!,
                        ExecutionDateTime = context.ExecutionDateTime!.Value,
                        Frequency = context.Frequency
                    });

                    await client.SendTextMessageAsync(userId, "Операция успешно добавлена ✅", 
                        cancellationToken: cancellationToken);
                    _stateService.ClearContext(userId);
                }
                else
                {
                    await client.SendTextMessageAsync(userId, "Неверный выбор. Введите номер из списка.", 
                        cancellationToken: cancellationToken);
                }
                break;
        }
    }
}