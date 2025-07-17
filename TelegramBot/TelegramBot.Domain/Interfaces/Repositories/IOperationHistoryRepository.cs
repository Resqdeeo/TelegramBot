using TelegramBot.Application.Entities;

namespace TelegramBot.Domain.Interfaces;

public interface IOperationHistoryRepository
{
    Task<IEnumerable<OperationHistory>> GetByOperationIdAsync(long operationId);
}