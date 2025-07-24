using Microsoft.EntityFrameworkCore;
using TelegramBot.Domain.Interfaces;
using TelegramBot.Infrastructure.Contexts;
using TelegramBot.Infrastructure.Repositories;

namespace TelegramBot.WebAPI.Configurations;

public static class PostgreConfiguration
{
    public static IServiceCollection AddPostgreConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IOperationHistoryRepository, OperationHistoryRepository>();
        
        return services;
    }
}