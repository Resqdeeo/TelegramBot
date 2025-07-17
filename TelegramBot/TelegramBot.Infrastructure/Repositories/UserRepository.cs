using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Entities;
using TelegramBot.Domain.Interfaces;
using TelegramBot.Infrastructure.Contexts;

namespace TelegramBot.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public Task<User?> GetByIdAsync(long id) =>
        _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<User?> GetByTelegramIdAsync(long telegramUserId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramUserId);
    }
    
    public async Task<bool> ExistsAsync(long telegramId)
    {
        return await _context.Users.AnyAsync(x => x.TelegramId == telegramId);
    }

    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}