using Telegram.Bot;

using Microsoft.EntityFrameworkCore;

using Sakura.Uwu.Models;

namespace Sakura.Uwu.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}