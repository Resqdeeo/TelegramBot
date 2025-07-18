using TelegramBot.Application.Abstractions;

namespace TelegramBot.Application.Interfaces.Services;

public interface IUserStateService
{
    bool HasContext(long userId);
    UserOperationContext GetContext(long userId);
    void SetContext(long userId, UserOperationContext context);
    void ClearContext(long userId);
}