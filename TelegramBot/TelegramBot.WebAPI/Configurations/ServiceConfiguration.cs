using Telegram.Bot;
using TelegramBot.Application.BotCommands;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Infrastructure.Services;
using TelegramBot.WebAPI.Services;

namespace TelegramBot.WebAPI.Configurations;

public static class ServiceConfiguration
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services)
    {
        
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") 
                       ?? throw new InvalidOperationException("TELEGRAM_TOKEN не установлен");
        services.AddSingleton<ITelegramBotClient>(sp => 
            new TelegramBotClient(botToken));
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IOperationHistoryService, OperationHistoryService>();
        
        services.AddSingleton<IUserStateService, UserStateService>();
        services.AddScoped<IBotCommand, StartCommand>();
        services.AddScoped<IBotCommand, ListOperationsCommand>();
        services.AddScoped<IBotCommand, AddOperationCommand>();
        services.AddScoped<IBotCommand, HelpCommand>();
        services.AddScoped<IBotCommand, DeleteOperationCommand>();
        services.AddScoped<IBotCommand, EditOperationCommand>();

        
        services.AddHostedService<TelegramBotService>();
        services.AddHostedService<NotificationService>();
        
        return services;
    }
}