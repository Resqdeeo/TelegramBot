using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Application.Utlis;

public static class InlineUtils
{
    public static InlineKeyboardMarkup GetYesNoKeyboard(string action, string id)
    {
        return new InlineKeyboardMarkup([
            InlineKeyboardButton.WithCallbackData("Да", $"{action}:{id}:yes"),
            InlineKeyboardButton.WithCallbackData("Нет", $"{action}:{id}:no")
        ]);
    }

    public static InlineKeyboardMarkup GetInlineKeyboard(Dictionary<long, string> buttons, string action)
    {
        return new InlineKeyboardMarkup(buttons.Select(o => new[]
        {
            InlineKeyboardButton.WithCallbackData(o.Value, $"{action}:{o.Key}")
        }).ToList());
    }
    
    public static InlineKeyboardMarkup GetInlineKeyboardBackButton(string action, string id)
    {
        return new InlineKeyboardMarkup([InlineKeyboardButton.WithCallbackData("Назад", $"{action}:{id}:back")]);
    }
}