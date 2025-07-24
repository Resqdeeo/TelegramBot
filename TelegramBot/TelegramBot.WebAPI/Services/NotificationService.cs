using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBot.Application.Entities;
using TelegramBot.Domain.Enums;
using TelegramBot.Domain.Interfaces;

namespace TelegramBot.WebAPI.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;
        private readonly IServiceProvider _services;
        private readonly ITelegramBotClient _botClient;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

        public NotificationService(
            ILogger<NotificationService> logger,
            IServiceProvider services,
            ITelegramBotClient botClient, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _botClient = botClient;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var operationRepo = scope.ServiceProvider.GetRequiredService<IOperationRepository>();

                    var now = DateTime.UtcNow + TimeSpan.FromHours(3);
                    // Получаем все активные операции (будущие и повторяющиеся)
                    var operations = await operationRepo.GetUpcomingOperationsAsync(now);

                    _logger.LogInformation($"Всего операций: {operations.Count()}");
                    
                    foreach (var op in operations)
                    {
                        _logger.LogInformation($"Обработка операции: {op.Title} " + "{0:yyyy-MM-dd HH:mm:ss.fff}", op.ExecutionDateTime);
                        await CheckOperationDueTimeAsync(op, now);
                        await ProcessOperationNotificationsAsync(op, now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в сервисе уведомлений");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessOperationNotificationsAsync(Operation op, DateTime now)
        {
            switch (op.Frequency)
            {
                case OperationFrequency.Once:
                    await CheckSingleOperationAsync(op, now);
                    break;
                case OperationFrequency.Hourly:
                    await CheckHourlyOperationAsync(op, now);
                    break;
                case OperationFrequency.Daily:
                    await CheckDailyOperationAsync(op, now);
                    break;
                case OperationFrequency.Weekly:
                    await CheckSingleOperationAsync(op, now);
                    break;
                case OperationFrequency.Monthly:
                    await CheckSingleOperationAsync(op, now);
                    break;
            }
        }

        private async Task CheckSingleOperationAsync(Operation op, DateTime now)
        {
            var timeRemaining = op.ExecutionDateTime - now;

            _logger.LogInformation($"Осталось {timeRemaining} до выполнения операции '{op.Title}'");
            
            if (timeRemaining <= TimeSpan.FromDays(1) && timeRemaining > TimeSpan.FromDays(1) - _checkInterval)
            {
                await SendNotificationAsync(op, $"Напоминание: через день наступит операция '{op.Title}'");
            }
            
            if (timeRemaining <= TimeSpan.FromHours(1) && timeRemaining > TimeSpan.FromHours(1) - _checkInterval)
            {
                await SendNotificationAsync(op, $"Напоминание: через час наступит операция '{op.Title}'");
            }
            
            if (timeRemaining <= TimeSpan.FromMinutes(15) && timeRemaining > TimeSpan.FromMinutes(15) - _checkInterval)
            {
                await SendNotificationAsync(op, $"Напоминание: через 15 минут наступит операция '{op.Title}'");
            }
        }
        
        private async Task CheckHourlyOperationAsync(Operation op, DateTime now)
        {
            var timeRemaining = op.ExecutionDateTime - now;

            _logger.LogInformation($"Осталось {timeRemaining} до выполнения операции '{op.Title}'");
            
            if (timeRemaining <= TimeSpan.FromMinutes(15) && timeRemaining > TimeSpan.FromMinutes(15) - _checkInterval)
            {
                await SendNotificationAsync(op, $"Напоминание: через 15 минут наступит операция '{op.Title}'");
            }
        }
        
        private async Task CheckDailyOperationAsync(Operation op, DateTime now)
        {
            var timeRemaining = op.ExecutionDateTime - now;

            _logger.LogInformation($"Осталось {timeRemaining} до выполнения операции '{op.Title}'");
            
            if (timeRemaining <= TimeSpan.FromHours(1) && timeRemaining > TimeSpan.FromHours(1) - _checkInterval)
            {
                await SendNotificationAsync(op, $"Напоминание: через час наступит операция '{op.Title}'");
            }
            
            if (timeRemaining <= TimeSpan.FromMinutes(15) && timeRemaining > TimeSpan.FromMinutes(15) - _checkInterval)
            {
                await SendNotificationAsync(op, $"Напоминание: через 15 минут наступит операция '{op.Title}'");
            }
        }
        

        private async Task SendNotificationAsync(Operation op, string message)
        {
            try
            {
                await _botClient.SendTextMessageAsync(
                    op.User.TelegramId,
                    message,
                    disableNotification: false);
                
                _logger.LogInformation($"Отправлено уведомление для операции {op.Id} пользователю {op.User.TelegramId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки уведомления для операции {op.Id}");
            }
        }
        
        private async Task CheckOperationDueTimeAsync(Operation op, DateTime now)
        {
            // Проверяем, что текущее время попадает в окно срабатывания
            if (now >= op.ExecutionDateTime - _checkInterval && 
                now <= op.ExecutionDateTime + _checkInterval)
            {
                string message = $"⏰ Время выполнить операцию: {op.Title}\n";
                if (!string.IsNullOrEmpty(op.Description))
                {
                    message += $"\n📝 Описание: {op.Description}";
                }
        
                await SendNotificationAsync(op, message);
            }
        }
    }
}