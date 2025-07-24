using TelegramBot.Application.Contracts;

namespace TelegramBot.Application.Interfaces.Services;

public interface IOperationService
{
    Task AddOperationAsync(long telegramId, OperationDto dto);
    Task<List<OperationDto>> GetUpcomingOperationsAsync(long telegramId, DateTime from, DateTime to);
    Task DeleteOperationAsync(long telegramId, long operationId);
    Task<List<OperationDto>> GetUserOperationsAsync(long telegramId);
    Task<OperationDto> GetOperationByIdAsync(long telegramId, long opId);
    Task UpdateOperationAsync(long telegramId, OperationDto operation);
    Task CompleteAndRescheduleOperationAsync(long telegramId, long operationId);
}