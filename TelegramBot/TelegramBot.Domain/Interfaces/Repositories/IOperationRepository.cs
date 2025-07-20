using TelegramBot.Application.Entities;

namespace TelegramBot.Domain.Interfaces;

public interface IOperationRepository 
{
    Task<Operation?> GetByIdAsync(long id);
    Task<IEnumerable<Operation>> GetByUserIdAsync(long userId);
    Task<IEnumerable<Operation>> GetPlannedForPeriodAsync(long userId, DateTime from, DateTime to);
    Task CreateOperationAsync(Operation operation);
    Task DeleteOperationAsync(Operation operation);
    Task UpdateOperationAsync();
    Task<IEnumerable<Operation>> GetAllAsync();
}
