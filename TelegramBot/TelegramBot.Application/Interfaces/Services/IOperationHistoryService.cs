using TelegramBot.Application.Contracts;

namespace TelegramBot.Application.Interfaces.Services;

public interface IOperationHistoryService
{
    Task<List<OperationHistoryDto>> GetOperationHistoryAsync(long userId);
}