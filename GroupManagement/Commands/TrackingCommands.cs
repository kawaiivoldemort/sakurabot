using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Sakura.Uwu.Services;
using Sakura.Uwu.Models;

namespace Sakura.Uwu.GroupManagement
{
    static partial class Commands
    {
        public static void Track(IBotService botService, Message message, BotDbContext dbContext)
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
            dbContext.SaveChanges();
        }
    }
}