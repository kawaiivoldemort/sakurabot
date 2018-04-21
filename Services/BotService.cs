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
            var databaseClient = new MongoClient(this.botSettings.DatabaseConnectionString);
            this.Database = databaseClient.GetDatabase(this.botSettings.DatabaseName);
        }

        public TelegramBotClient Client { get; }
        public IMongoDatabase Database { get; }
    }
}