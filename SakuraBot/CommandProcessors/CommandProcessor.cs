using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Sakura.Uwu.Services;
using Sakura.Uwu.Models;

namespace Sakura.Uwu.CommandProcessors
{
    public delegate Task CommandTask
    (
        Message message,
        ServicesContext serviceContext,
        BotDbContext dbContext
    );
    public class Command
    {
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public CommandTask TaskProcess { get; set; }
    }
    public interface ICommandProcessor
    {
        string Name { get; }
        UpdateType Type { get; }
        Dictionary<string, Command> Commands { get; }
        bool IsFinalCommand { get; }
        bool DoesProcessCommand(Update update);
        Task ProcessCommand(Message message, ServicesContext context, BotDbContext dbContext);
        string GetDescriptions();
    }
}