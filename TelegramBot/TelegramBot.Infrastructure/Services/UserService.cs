using TelegramBot.Application.Entities;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Interfaces;

namespace TelegramBot.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task CreateUserIfNotExistsAsync(long telegramId, string username)
    {
        var exists = await _userRepository.ExistsAsync(telegramId);
        if (!exists)
        {
            var user = new User
            {
                TelegramId = telegramId,
                UserName = username,
                RegisteredAt = DateTime.UtcNow
            };
            await _userRepository.CreateUserAsync(user);
        }
    }

    public Task<bool> UserExistsAsync(long telegramId)
    {
        return _userRepository.ExistsAsync(telegramId);
    }
}