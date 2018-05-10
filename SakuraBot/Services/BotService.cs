using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using Telegram.Bot;

using Sakura.Uwu.Models;

namespace Sakura.Uwu.Services
{
    // Bot Service to handle Telegram Client connections and Database Connections
    public class BotService : IBotService
    {
        private readonly BotSettings botSettings;
        public BotService(IOptions<BotSettings> botSettings)
        {
            this.botSettings = botSettings.Value;
            this.Client = new TelegramBotClient(this.botSettings.BotToken);
        }
        public TelegramBotClient Client { get; }
    }
}