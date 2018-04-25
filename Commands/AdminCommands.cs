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
        public static readonly Dictionary<string, Command> Admin = new Dictionary<string, Command>
        {
            { "/warn", WarnUserCommand },
            { "/clearwarns", ClearWarnsCommand },
            { "/ban", BanCommand },
            { "/kick", KickCommand },
            { "/unban", UnbanCommand }
        };

        private static async Task WarnUserCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
            var table = dbContext.Warns;
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese repwy to the offensive message >w<!",
                    replyToMessageId: message.MessageId
                );
            }
            else if (originMessage.From.IsBot)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "nuu >w< can't warn senpai!",
                    replyToMessageId: message.MessageId
                );

            }
            else if (admins.Any(admin => admin.User.Id == originMessage.From.Id))
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "nuu >w< can't warn admin-sama!",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var result = table.FirstOrDefault(x => x.UserId == originMessage.From.Id);
                if (result == null)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Warned
<b>{originMessage.From.FirstName} {originMessage.From.LastName}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>

Warn Count: 1",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                    table.Add(new UserWarns(originMessage.From.Id));
                }
                else
                {
                    result.WarnCount += 1;
                    if (result.WarnCount == 3)
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
$@"Warn limit reached! Kicked
<b>{originMessage.From.FirstName} {originMessage.From.LastName}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>
    
UwU",
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        await client.KickChatMemberAsync(message.Chat.Id, originMessage.From.Id);
                    }
                    else
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
$@"Warned
<b>{originMessage.From.FirstName} {originMessage.From.LastName}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>

Warn Count: {result.WarnCount}",
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                    }
                }
            }
            dbContext.SaveChanges();
        }

        private static async Task ClearWarnsCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var originMessage = message.ReplyToMessage;
            var client = botService.Client;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese repwy to the offensive message >w<!",
                    replyToMessageId: message.MessageId
                );
            }
            else {
                var table = dbContext.Warns;
                var result = table.FirstOrDefault(x => x.UserId == originMessage.From.Id);
                if (result != null)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Warns reset for
<b>{originMessage.From.FirstName} {originMessage.From.LastName}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                    result.WarnCount += 1;
                }
            }
            dbContext.SaveChanges();
        }

        private static async Task BanCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese repwy to the offensive message >w<!",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var originUser = originMessage.From;
                if (admins.Any(admin => admin.User.Id == originUser.Id))
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        "nuu >w< can't ban admin-sama!",
                        replyToMessageId: message.MessageId
                    );
                }
                else
                {
                    if (originUser.IsBot)
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
                            "I'm NOT banning senpai! >w<",
                            replyToMessageId: message.MessageId
                        );
                    }
                    else
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
$@"Banned
<b>{originMessage.From.FirstName} {originMessage.From.LastName}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>",
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        await client.KickChatMemberAsync(message.Chat.Id, originUser.Id);
                    }
                }
            }
        }

        private static async Task KickCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese repwy to the offensive message >w<!",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var originUser = originMessage.From;
                if (admins.Any(admin => admin.User.Id == originUser.Id))
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        "nuu >w< can't kick admin-sama!",
                        replyToMessageId: message.MessageId
                    );
                }
                else
                {
                    if (originUser.IsBot)
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
                            "I'm NOT kicking senpai! >w<",
                            replyToMessageId: message.MessageId
                        );
                    }
                    else
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
$@"Kicked
<b>{originMessage.From.FirstName} {originMessage.From.LastName}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>

But they can rejoin in a minute UwU!",
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        await client.KickChatMemberAsync(message.Chat.Id, originUser.Id, System.DateTime.Now.AddMinutes(1));
                    }
                }
            }
        }

        private static async Task UnbanCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese repwy to the users message UwU!",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var originUser = originMessage.From;
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
$@"Unbanned
<b>{originMessage.From.FirstName} {originMessage.From.LastName}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>",
                    parseMode: ParseMode.Html
                );
                await client.UnbanChatMemberAsync(message.Chat.Id, originUser.Id);
            }
        }
    }
}
