using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.Contracts;

public class OperationHistoryDto
{
    public string OperationTitle { get; set; }
    public string OperationDescription { get; set; }
    public DateTime PerformedAt { get; set; }
    public OperationFrequency Frequency { get; set; }
}