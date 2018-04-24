using Microsoft.Extensions.Options;

using Telegram.Bot;
using MongoDB.Driver;

using Sakura.Uwu.Models;

namespace Sakura.Uwu.Services {
    // Bot Service to handle Telegram Client connections and Database Connections
    public class BotService : IBotService {
        private readonly BotSettings botSettings;
        public BotService(IOptions<BotSettings> botSettings) {
            this.botSettings = botSettings.Value;
            this.Client = new TelegramBotClient(this.botSettings.BotToken);
            this.Dbms = new MongoClient(this.botSettings.DatabaseConnectionString);
        }

        public TelegramBotClient Client { get; }
        public MongoClient Dbms { get; }
    }
}