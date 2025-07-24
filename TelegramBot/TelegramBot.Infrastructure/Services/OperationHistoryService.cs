using TelegramBot.Application.Contracts;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Interfaces;

namespace TelegramBot.Infrastructure.Services;

public class OperationHistoryService : IOperationHistoryService
{
    private readonly IOperationHistoryRepository _historyRepo;
    private readonly IUserRepository _userRepository;
    
    public OperationHistoryService(IOperationHistoryRepository historyRepo, IUserRepository userRepository)
    {
        _historyRepo = historyRepo;
        _userRepository = userRepository;
    }
    
    public async Task<List<OperationHistoryDto>> GetOperationHistoryAsync(long telegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return new List<OperationHistoryDto>();
        
        var history = await _historyRepo.GetByUserIdAsync(user.Id);
        
        return history.Select(h => new OperationHistoryDto
        {
            OperationTitle = h.Operation.Title,
            OperationDescription = h.Operation.Description,
            PerformedAt = h.PerformedAt,
            Frequency = h.Operation.Frequency
        }).ToList();
    }
}