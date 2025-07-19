using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Entities;
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
}