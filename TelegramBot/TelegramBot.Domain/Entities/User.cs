using TelegramBot.Domain.Common;

namespace TelegramBot.Application.Entities;

/// <summary>
/// Сущность пользователя
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Id пользователя telegram
    /// </summary>
    public long TelegramId { get; set; }
    
    /// <summary>
    /// Username 
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Дата регистрации
    /// </summary>
    public DateTime RegisteredAt { get; set; }
    
    /// <summary>
    /// Набор операций пользователя
    /// </summary>
    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}