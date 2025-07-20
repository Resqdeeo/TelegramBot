using Quartz;
using Telegram.Bot;
using TelegramBot.Domain.Enums;
using TelegramBot.Domain.Interfaces;

namespace TelegramBot.Application.Jobs;

public class PeriodicMessageJob : IJob
{
    private readonly ITelegramBotClient _botClient;
    private readonly IOperationRepository _operationRepository;

    public PeriodicMessageJob(ITelegramBotClient botClient, IOperationRepository operationRepository)
    {
        _botClient = botClient;
        _operationRepository = operationRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobId = context.JobDetail.Key.Name;
        var opId = int.Parse(jobId.Replace("message_", ""));
        var operation = await _operationRepository.GetByIdAsync(opId);

        if (operation == null) return;

        try
        {
            await _botClient.SendTextMessageAsync(
                chatId: operation.User.TelegramId,
                text: operation.Title);

            if (operation.Frequency == OperationFrequency.Once)
            {
                await _operationRepository.DeleteOperationAsync(operation);
                await context.Scheduler.DeleteJob(context.JobDetail.Key);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке сообщения {operation.Id}: {ex.Message}");
        }
    }
}