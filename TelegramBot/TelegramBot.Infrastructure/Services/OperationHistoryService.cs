using TelegramBot.Application.Contracts;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Interfaces;

namespace TelegramBot.Infrastructure.Services;

public class OperationHistoryService : IOperationHistoryService
{
    private readonly IOperationHistoryRepository _historyRepo;
    
    public OperationHistoryService(IOperationHistoryRepository historyRepo)
    {
        _historyRepo = historyRepo;
    }
    
    public async Task<List<OperationHistoryDto>> GetOperationHistoryAsync(long userId)
    {
        var history = await _historyRepo.GetByUserIdAsync(userId);
        
        return history.Select(h => new OperationHistoryDto
        {
            OperationTitle = h.Operation.Title,
            OperationDescription = h.Operation.Description,
            PerformedAt = h.PerformedAt,
            Frequency = h.Operation.Frequency
        }).ToList();
    }
}