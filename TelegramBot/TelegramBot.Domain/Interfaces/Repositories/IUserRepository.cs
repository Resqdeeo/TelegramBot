using TelegramBot.Application.Entities;

namespace TelegramBot.Domain.Interfaces;

public interface IUserRepository 
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByTelegramIdAsync(long telegramUserId);
    Task<bool> ExistsAsync(long telegramId);
    Task CreateUserAsync(User user);
    Task DeleteUserAsync(User user);
}