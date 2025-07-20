namespace TelegramBot.Application.Abstractions;

public enum EditOperationStep
{
    None,
    EditChoice,
    EditTitle,
    EditDescription,
    EditDate,
    EditTime,
}