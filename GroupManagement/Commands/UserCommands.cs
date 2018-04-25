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
        public static readonly Dictionary<string, Command> User = new Dictionary<string, Command>
        {
            { "/start", StartCommand },
            { "/whoami", WhoAmICommand },
            { "/admins", ListAdminsCommand }
        };
        private static async Task StartCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
            var chat = await client.GetChatAsync(message.Chat.Id);
            await client.SendTextMessageAsync
            (
                message.Chat.Id,
$@"I smell a new chat!
<b>{chat.FirstName} {chat.LastName}</b>
@{chat.Username}
<code>{chat.Id}</code>",
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html
            );
        }
        private static async Task WhoAmICommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
            var profilePhotos = await client.GetUserProfilePhotosAsync(message.From.Id, 0, 1);
            if (profilePhotos.TotalCount != 0)
            {
                await client.SendPhotoAsync
                (
                    message.Chat.Id,
                    profilePhotos.Photos[0][0].FileId,
$@"You are
<b>{message.From.FirstName} {message.From.LastName}{(message.From.IsBot ? "🤖" : "")}</b>
@{message.From.Username}
<code>{message.From.Id}</code>",
                    parseMode: ParseMode.Html
                );
            }
            else
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
$@"You are
<b>{message.From.FirstName} {message.From.LastName}{(message.From.IsBot ? "🤖" : "")}</b>
@{message.From.Username}
<code>{message.From.Id}</code>",
                    parseMode: ParseMode.Html
                );
                
            }
        }

        private static async Task ListAdminsCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var adminString = admins
                .Select
                (
                    (admin) =>
$@"• <b>{admin.User.FirstName} {admin.User.LastName}{(admin.User.IsBot ? "🤖" : "")}</b>
  @{admin.User.Username}
  <code>{admin.User.Id}</code>"
                )
                .Aggregate
                (
                    (s1, s2) => s1 + "\n\n" + s2
                );
            await client.SendTextMessageAsync
            (
                message.Chat.Id,
                adminString,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html
            );
        }
    }
}
