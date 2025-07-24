using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.Contracts;

public class OperationDto
{
    public string Title { get; set; } 
    public string Description { get; set; } 
    public DateTime ExecutionDateTime { get; set; }
    public OperationFrequency Frequency { get; set; }
    public long Id { get; set; }
}