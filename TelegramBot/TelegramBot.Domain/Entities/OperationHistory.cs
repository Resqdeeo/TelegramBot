using TelegramBot.Domain.Common;

namespace TelegramBot.Application.Entities;

/// <summary>
/// Сущность истории операции
/// </summary>
public class OperationHistory : BaseEntity
{
    /// <summary>
    /// Id операции
    /// </summary>
    public long OperationId { get; set; }
    
    /// <summary>
    /// Ссылка на операцию
    /// </summary>
    public Operation Operation { get; set; }
    
    /// <summary>
    /// Дата-время выступления
    /// </summary>
    public DateTime PerformedAt { get; set; }
}