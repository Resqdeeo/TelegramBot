using TelegramBot.Application.Abstractions;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Infrastructure.Services;

public class UserStateService : IUserStateService
{
    private readonly Dictionary<long, UserOperationContext> _userStates = new();

    public bool HasContext(long userId) => _userStates.ContainsKey(userId);

    public UserOperationContext GetContext(long userId)
    {
        if (!_userStates.ContainsKey(userId))
            _userStates[userId] = new UserOperationContext();

        return _userStates[userId];
    }

    public void SetContext(long userId, UserOperationContext context)
    {
        _userStates[userId] = context;
    }

    public void ClearContext(long userId)
    {
        _userStates.Remove(userId);
    }
}