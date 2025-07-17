using TelegramBot.Application.Entities;
using TelegramBot.Domain.Interfaces;
using TelegramBot.Infrastructure.Contexts;

namespace TelegramBot.Infrastructure.Repositories;

public class OperationHistoryRepository : IOperationHistoryRepository
{
    private readonly AppDbContext _context;

    public OperationHistoryRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public Task<IEnumerable<OperationHistory>> GetByOperationIdAsync(long operationId) =>
        Task.FromResult<IEnumerable<OperationHistory>>(
            _context.OperationHistories
                .Where(h => h.OperationId == operationId)
                .ToList());

    public async Task CreateAsync(OperationHistory history)
    {
        _context.OperationHistories.Add(history);
        await _context.SaveChangesAsync();
    }
}