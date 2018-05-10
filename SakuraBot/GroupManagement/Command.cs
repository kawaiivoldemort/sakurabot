using System.Threading.Tasks;

using Telegram.Bot.Types;

using Sakura.Uwu.Services;
using Sakura.Uwu.Models;

namespace Sakura.Uwu.GroupManagement
{
    public delegate Task Command(IBotService botService, Message message, BotDbContext dbContext);
}