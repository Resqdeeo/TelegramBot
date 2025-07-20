using Quartz;
using TelegramBot.Application.Contracts;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Application.Jobs;
using TelegramBot.Domain.Enums;

namespace TelegramBot.Infrastructure.Schedulers;

public class MessageScheduler : IMessageScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private IOperationService _operationService;

    public MessageScheduler(ISchedulerFactory schedulerFactory, IOperationService operationService)
    {
        _schedulerFactory = schedulerFactory;
        _operationService = operationService;
    }

    public async Task ScheduleMessage(OperationDto operation)
    {
        var scheduler = await _schedulerFactory.GetScheduler();

        var job = JobBuilder.Create<PeriodicMessageJob>()
            .WithIdentity($"message_{operation.Id}", "periodic_messages")
            .Build();

        ITrigger trigger = operation.Frequency switch
        {
            OperationFrequency.Hourly => CreateHourlyTrigger(operation),
            OperationFrequency.Daily => CreateDailyTrigger(operation),
            OperationFrequency.Weekly => CreateWeeklyTrigger(operation),
            OperationFrequency.Monthly => CreateMonthlyTrigger(operation),
            OperationFrequency.Yearly => CreateYearlyTrigger(operation),
            _ => CreateOneTimeTrigger(operation)
        };

        await scheduler.ScheduleJob(job, trigger);
    }

    private ITrigger CreateYearlyTrigger(OperationDto operation)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"trigger_{operation.Id}", "periodic_messages")
            .StartAt(operation.ExecutionDateTime)
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(24 * 365)
                .RepeatForever())
            .Build();
    }

    private ITrigger CreateHourlyTrigger(OperationDto operation)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"trigger_{operation.Id}", "periodic_messages")
            .StartAt(operation.ExecutionDateTime)
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(1)
                .RepeatForever())
            .Build();
    }

    private ITrigger CreateOneTimeTrigger(OperationDto operation)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"trigger_{operation.Id}", "periodic_messages")
            .StartAt(operation.ExecutionDateTime)
            .Build();
    }

    private ITrigger CreateDailyTrigger(OperationDto operation)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"trigger_{operation.Id}", "periodic_messages")
            .StartAt(operation.ExecutionDateTime)
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(24)
                .RepeatForever())
            .Build();
    }

    private ITrigger CreateWeeklyTrigger(OperationDto operation)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"trigger_{operation.Id}", "periodic_messages")
            .StartAt(operation.ExecutionDateTime)
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(24 * 7)
                .RepeatForever())
            .Build();
    }

    private ITrigger CreateMonthlyTrigger(OperationDto operation)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"trigger_{operation.Id}", "periodic_messages")
            .StartAt(operation.ExecutionDateTime)
            .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(
                operation.ExecutionDateTime.Day,
                operation.ExecutionDateTime.Hour,
                operation.ExecutionDateTime.Minute))
            .Build();
    }

    public async Task StartAsync()
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.Start();

        // Восстановление активных задач
        var activeMessages = await _operationService.GetAllAsync();
        foreach (var message in activeMessages)
        {
            await ScheduleMessage(message);
        }
    }
}