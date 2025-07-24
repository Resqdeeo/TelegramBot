using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.Abstractions;

public class UserOperationContext
{
    public DeleteOperationStep DeleteStep { get; set; } = DeleteOperationStep.None;
    public AddOperationStep Step { get; set; } = AddOperationStep.None;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ExecutionDateTime { get; set; }
    public OperationFrequency Frequency { get; set; }
    public EditOperationStep EditStep { get; set; } = EditOperationStep.None;
    public long Id { get; set; }
}