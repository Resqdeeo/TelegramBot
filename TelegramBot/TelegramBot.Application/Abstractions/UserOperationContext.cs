using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.Abstractions;

public class UserOperationContext
{
    public AddOperationStep Step { get; set; } = AddOperationStep.None;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ExecutionDateTime { get; set; }
    public OperationFrequency Frequency { get; set; }
}