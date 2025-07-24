using TelegramBot.Application.Contracts;
using TelegramBot.Application.Entities;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Interfaces;

namespace TelegramBot.Infrastructure.Services;

public class OperationService : IOperationService
{
    private readonly IUserRepository _userRepo;
    private readonly IOperationRepository _operationRepo;

    public OperationService(IUserRepository userRepo, IOperationRepository operationRepo)
    {
        _userRepo = userRepo;
        _operationRepo = operationRepo;
    }

    public async Task AddOperationAsync(long telegramId, OperationDto dto)
    {
        var user = await _userRepo.GetByTelegramIdAsync(telegramId);
        if (user == null)
            throw new Exception("User not found");

        var operation = new Operation
        {
            Title = dto.Title,
            Description = dto.Description,
            ExecutionDateTime = dto.ExecutionDateTime,
            Frequency = dto.Frequency,
            UserId = user.Id
        };

        await _operationRepo.CreateOperationAsync(operation);
    }

    public async Task<List<OperationDto>> GetUserOperationsAsync(long telegramId)
    {
        var user = await _userRepo.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return new();

        var operations = await _operationRepo.GetByUserIdAsync(user.Id);

        return operations.Select(o => new OperationDto
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            ExecutionDateTime = o.ExecutionDateTime,
            Frequency = o.Frequency
        }).ToList();
    }

    public async Task<OperationDto> GetOperationByIdAsync(long telegramId, long opId)
    {
        var user = await _userRepo.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return new();


        var operation = await _operationRepo.GetByIdAsync(opId);
        if (operation != null && operation.UserId == user.Id)
        {
            return new OperationDto
            {
                Id = operation.Id,
                Title = operation.Title,
                Description = operation.Description,
                ExecutionDateTime = operation.ExecutionDateTime,
                Frequency = operation.Frequency
            };
        }

        return new();
    }

    public async Task UpdateOperationAsync(long telegramId, OperationDto operation)
    {
        var user = await _userRepo.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return;
        
        var operationDb = await _operationRepo.GetByIdAsync(operation.Id);
        if (operationDb != null && operationDb.UserId == user.Id)
        {
            operationDb.Description = operation.Description;
            operationDb.Frequency = operation.Frequency;
            operationDb.Title = operation.Title;
            operationDb.ExecutionDateTime = operation.ExecutionDateTime;
            await _operationRepo.UpdateOperationAsync();
        }
    }

    public async Task MarkOperationAsCompletedAsync(long telegramId, long operationId)
    {
        var user = await _userRepo.GetByTelegramIdAsync(telegramId);
        if (user == null)
            throw new Exception("User not found");

        var operation = await _operationRepo.GetByIdAsync(operationId);
        if (operation != null && operation.UserId == user.Id)
        {
            await _operationRepo.MarkAsCompletedAsync(operationId);
        }
    }

    public async Task DeleteOperationAsync(long telegramId, long operationId)
    {
        var user = await _userRepo.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return;

        var operation = await _operationRepo.GetByIdAsync(operationId);
        if (operation != null && operation.UserId == user.Id)
        {
            await _operationRepo.DeleteOperationAsync(operation);
        }
    }

    public async Task<List<OperationDto>> GetUpcomingOperationsAsync(long telegramId, DateTime from, DateTime to)
    {
        var user = await _userRepo.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return new();

        var operations = await _operationRepo.GetPlannedForPeriodAsync(user.Id, from, to);

        return operations.Select(o => new OperationDto
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            ExecutionDateTime = o.ExecutionDateTime,
            Frequency = o.Frequency
        }).ToList();
    }
}