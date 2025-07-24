namespace TelegramBot.Application.Abstractions;

public enum AddOperationStep
{
    None,
    AwaitingTitle,
    AwaitingDescription,
    AwaitingDateTime,
    AwaitingFrequency
}