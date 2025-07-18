namespace TelegramBot.Application.Interfaces.Services;

public interface IUserService
{
    Task CreateUserIfNotExistsAsync(long telegramId, string username);
    Task<bool> UserExistsAsync(long telegramId);
}