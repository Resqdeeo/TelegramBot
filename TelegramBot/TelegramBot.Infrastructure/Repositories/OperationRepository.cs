using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Entities;
using TelegramBot.Domain.Enums;
using TelegramBot.Domain.Interfaces;
using TelegramBot.Infrastructure.Contexts;

namespace TelegramBot.Infrastructure.Repositories;

public class OperationRepository : IOperationRepository
{
    private readonly AppDbContext _context;

    public OperationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Operation?> GetByIdAsync(long id)
    {
        return await _context.Operations.Include(o => o.History).FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<IEnumerable<Operation>> GetByUserIdAsync(long userId)
    {
        return Task.FromResult<IEnumerable<Operation>>(
            _context.Operations.Where(o => o.UserId == userId).ToList());
    }

    public Task<IEnumerable<Operation>> GetPlannedForPeriodAsync(long userId, DateTime from, DateTime to)
    {
        return Task.FromResult<IEnumerable<Operation>>(
            _context.Operations
                .Where(o => o.UserId == userId && o.ExecutionDateTime >= from && o.ExecutionDateTime <= to)
                .ToList());
    }

    public async Task CreateOperationAsync(Operation operation)
    {
        _context.Operations.Add(operation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOperationAsync(Operation operation)
    {
        _context.Operations.Remove(operation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOperationAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task CompleteAndRescheduleOperationAsync(long operationId)
    {
        var operation = await _context.Operations
            .Include(o => o.History)
            .FirstOrDefaultAsync(o => o.Id == operationId);

        if (operation != null)
        {
            operation.History.Add(new OperationHistory
            {
                OperationId = operationId,
                PerformedAt = operation.ExecutionDateTime
            });

            operation.ExecutionDateTime = operation.Frequency switch
            {
                OperationFrequency.Hourly => operation.ExecutionDateTime.AddHours(1),
                OperationFrequency.Daily => operation.ExecutionDateTime.AddDays(1),
                OperationFrequency.Weekly => operation.ExecutionDateTime.AddDays(7),
                OperationFrequency.Monthly => operation.ExecutionDateTime.AddMonths(1),
                OperationFrequency.Yearly => operation.ExecutionDateTime.AddYears(1),
                _ => operation.ExecutionDateTime
            };

            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Operation>> GetUpcomingOperationsAsync(DateTime currentTime)
    {
        return await _context.Operations
            .Include(o => o.User)
            .Where(o =>
                // Будущие разовые операции
                (o.Frequency == OperationFrequency.Once && o.ExecutionDateTime > currentTime) ||
                // Или все повторяющиеся операции
                (o.Frequency != OperationFrequency.Once))
            .ToListAsync();
    }
}