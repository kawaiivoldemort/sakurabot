using Telegram.Bot;

using MongoDB.Driver;

namespace Sakura.Uwu.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
        MongoClient Dbms { get;}
    }
}