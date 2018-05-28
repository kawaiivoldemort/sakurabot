using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Sakura.Uwu.Models;
using Sakura.Uwu.Services;

namespace Sakura.Uwu.CommandProcessors
{
    class TrackingCommandProcessor : ICommandProcessor
    {
        public string Name { get; }
        public UpdateType Type { get; }
        public Dictionary<string, Command> Commands { get; }
        public bool IsFinalCommand { get; }
        public TrackingCommandProcessor()
        {
            Name = "User Tracking Silent Command Processor";
            Type = UpdateType.Message;
            Commands = new Dictionary<string, Command>{};
            IsFinalCommand = false;
        }
        public bool DoesProcessCommand(Update update)
        {
            return true;
        }
        public async Task ProcessCommand(Message message, ServicesContext context, BotDbContext dbContext)
        {
            try
            {
                var lookupTable = dbContext.Lookup;
                var entry = lookupTable.FirstOrDefault(user => user.UserId == message.From.Id);
                if(message.From.Username != null) {
                    if(entry == null) {
                        lookupTable.Add(new UserLookup(message.From.Id, message.From.Username));
                    } else if(entry.UserName != message.From.Username) {
                        entry.UserName = message.From.Username;
                    }
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in Tracking : {0}", e.Message);
            }
        }
        public string GetDescriptions()
        {
            return "";
        }
    }
}