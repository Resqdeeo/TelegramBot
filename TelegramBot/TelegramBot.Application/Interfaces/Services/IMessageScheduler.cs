using TelegramBot.Application.Contracts;

namespace TelegramBot.Application.Interfaces.Services;

public interface IMessageScheduler
{
    public Task ScheduleMessage(OperationDto operation);
    public Task StartAsync();
}