using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using MongoDB.Driver;
using MongoDB.Bson;

using Sakura.Uwu.Services;
using Sakura.Uwu.Models;

public delegate Task Command(IBotService botService, Message message);

namespace Sakura.Uwu.GroupManagement
{
    static class Commands
    {
        public static readonly Dictionary<string, Command> User = new Dictionary<string, Command>
        {
            { "/whoami", WhoAmICommand },
            { "/admins", ListAdminsCommand }
        };
        public static readonly Dictionary<string, Command> Admin = new Dictionary<string, Command>
        {
            { "/warn", WarnUserCommand },
            { "/clearwarns", ClearWarnsCommand },
            { "/ban", BanCommand },
            { "/kick", KickCommand },
            { "/unban", UnbanCommand }
        };

        private static async Task WhoAmICommand(IBotService botService, Message message)
        {
            var client = botService.Client;
            await client.SendTextMessageAsync
            (
                message.Chat.Id,
$@"You are
<b>{message.From.FirstName} {message.From.LastName}{(message.From.IsBot ? "🤖" : "")}</b>
@{message.From.Username}
<code>{message.From.Id}</code>",
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html
            );
        }

        private static async Task ListAdminsCommand(IBotService botService, Message message)
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

        private static async Task WarnUserCommand(IBotService botService, Message message)
        {
            var client = botService.Client;
            var dbms = botService.Dbms;
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
                var database = dbms.GetDatabase("UserRecords");
                var warnsCollection = database.GetCollection<UserWarns>("Warns");
                var filter = new BsonDocument("UserId", originMessage.From.Id);
                var results = warnsCollection.FindSync(filter).ToList().ToArray();
                if (results.Length == 0)
                {
                    var document = new UserWarns(originMessage.From.Id);
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
                    warnsCollection.InsertOne(document);
                }
                else
                {
                    var warnCount = results[0].WarnCount + 1;
                    if (warnCount == 3)
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

Warn Count: {warnCount}",
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                    }
                    var up = new BsonDocument("$set", new BsonDocument("WarnCount", warnCount));
                    warnsCollection.UpdateOne(filter, up);
                }
            }
        }

        private static async Task ClearWarnsCommand(IBotService botService, Message message)
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
                var dbms = botService.Dbms;
                var database = dbms.GetDatabase("UserRecords");
                var warnsCollection = database.GetCollection<UserWarns>("Warns");
                var filter = new BsonDocument("UserId", originMessage.From.Id);
                var results = warnsCollection.FindSync(filter).ToList().ToArray();
                if (results.Length != 0)
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
                    var up = new BsonDocument("$set", new BsonDocument("WarnCount", 0));
                    warnsCollection.UpdateOne(filter, up);
                }
            }
        }

        private static async Task BanCommand(IBotService botService, Message message)
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

        private static async Task KickCommand(IBotService botService, Message message)
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

        private static async Task UnbanCommand(IBotService botService, Message message)
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
