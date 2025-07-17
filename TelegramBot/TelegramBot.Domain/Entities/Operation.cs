using TelegramBot.Domain.Common;
using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.Entities;

/// <summary>
/// Сущность операции
/// </summary>
public class Operation : BaseEntity
{
    /// <summary>
    /// Id пользователя, которому принадлежит операция
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// Пользователь
    /// </summary>
    public User User { get; set; }
    
    /// <summary>
    /// Название операции
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Описание
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Дата-время выполнения операции
    /// </summary>
    public DateTime ExecutionDateTime { get; set; }
    
    /// <summary>
    /// Периодичность операции
    /// </summary>
    public OperationFrequency Frequency { get; set; }
    
    public List<OperationHistory> History { get; set; } = new();
}