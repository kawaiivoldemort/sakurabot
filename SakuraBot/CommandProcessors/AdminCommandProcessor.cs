using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

using Sakura.Uwu.Models;
using Sakura.Uwu.Services;

namespace Sakura.Uwu.CommandProcessors
{
    class AdminCommandProcessor : ICommandProcessor
    {
        public string Name { get; }
        public UpdateType Type { get; }
        public Dictionary<string, Command> Commands { get; }
        public bool IsFinalCommand { get; }
        public AdminCommandProcessor()
        {
            Name = "Administrator Commands";
            Type = UpdateType.Message;
            Commands = new Dictionary<string, Command>
            {
                { "/warn",              new Command() { TaskName="/warn",               TaskDescription="Warns User. Kicks on 3 Warns.",                          TaskProcess=WarnUserCommand           } },
                { "/clearwarns",        new Command() { TaskName="/clearwarns",         TaskDescription="Forgives a User and Sets Warns to 0",                    TaskProcess=ClearWarnsCommand         } },
                { "/ban",               new Command() { TaskName="/ban",                TaskDescription="Bans a User from the Chat",                              TaskProcess=BanCommand                } },
                { "/unban",             new Command() { TaskName="/unban",              TaskDescription="Unbans a User from the Chat",                            TaskProcess=UnbanCommand              } },
                { "/kick",              new Command() { TaskName="/kick",               TaskDescription="Kicks a User from the Chat for 1 Minute",                TaskProcess=KickCommand               } },
                { "/pin",               new Command() { TaskName="/pin",                TaskDescription="Pins a message in a group",                              TaskProcess=PinCommand                } },
                { "/loudpin",           new Command() { TaskName="/loudpin",            TaskDescription="Pins a message in a group and Notify all Chat Members",  TaskProcess=PinLoudlyCommand          } },
                { "/setwelcometext",    new Command() { TaskName="/setwelcometext",     TaskDescription="Sets Group Welcome Text Message",                        TaskProcess=SetWelcomeMessageCommand  } },
                { "/setwelcomerules",   new Command() { TaskName="/setwelcomerules",    TaskDescription="Sets Group Welcome Rules Message",                       TaskProcess=SetRulesCommand           } },
                { "/setwelcomemedia",   new Command() { TaskName="/setwelcomemedia",    TaskDescription="Sets Group Welcome Media Message",                       TaskProcess=SetWelcomeMediaCommand    } },
                { "/clearwelcome",      new Command() { TaskName="/clearwelcome",       TaskDescription="Clears Group Welcome Message",                           TaskProcess=ClearWelcomeCommand       } },
                { "/save",              new Command() { TaskName="/save",               TaskDescription="Save Message for Future Reference",                      TaskProcess=SaveMessage               } },
                { "/saved",             new Command() { TaskName="/saved",              TaskDescription="List Saved Messages",                                    TaskProcess=SavedMessage              } },
                { "/clearsaved",        new Command() { TaskName="/clearsaved",         TaskDescription="Clear Saved Messages",                                   TaskProcess=ClearSavedMessage         } }
            };
            IsFinalCommand = true;
        }
        public bool DoesProcessCommand(Update update)
        {
            if (update.Message.Type == MessageType.Text)
            {
                var messageParts = update.Message.Text.Split(' ');
                if (this.Commands.ContainsKey(messageParts[0]))
                {
                    return true;
                }
            }
            return false;
        }
        public async Task ProcessCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var admins = await serviceContext.TelegramBotService.Client.GetChatAdministratorsAsync(message.Chat.Id);
            if (admins.Any(admin => admin.User.Id == message.From.Id))
            {
                var self = await serviceContext.TelegramBotService.Client.GetMeAsync();
                var selfChatMember = await serviceContext.TelegramBotService.Client.GetChatMemberAsync(message.Chat.Id, self.Id);
                if
                (
                    selfChatMember.CanPinMessages != null &&
                    selfChatMember.CanRestrictMembers != null &&
                    (bool)selfChatMember.CanPinMessages &&
                    (bool)selfChatMember.CanRestrictMembers
                )
                {
                    var messageParts = message.Text.Split(' ');
                    await this.Commands[messageParts[0]].TaskProcess(message, serviceContext, dbContext);
                }
                else
                {
                    await serviceContext.TelegramBotService.Client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        "I cannot do that, ish not admin >.>",
                        replyToMessageId: message.MessageId
                    );
                }
            }
            else
            {
                await serviceContext.TelegramBotService.Client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    ">w< Senpai, you aren't admin!",
                    replyToMessageId: message.MessageId
                );
            }
        }
        public string GetDescriptions()
        {
            var descriptions = new StringBuilder($"--------\n<b>{Name}</b>\n\n");
            foreach (var cmd in this.Commands)
            {
                descriptions.AppendLine($"<b>{cmd.Value.TaskName}</b>: <i>{cmd.Value.TaskDescription}</i>");
            }
            return descriptions.ToString();
        }
        private async Task WarnUserCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var table = dbContext.Warns;
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to a message >.> baka.",
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
                var result = table.FirstOrDefault(x => x.UserId == originMessage.From.Id && x.GroupId == originMessage.Chat.Id);
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
                    table.Add(new UserWarns(originMessage.Chat.Id, originMessage.From.Id));
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
    
^-^",
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        try
                        {
                            await client.KickChatMemberAsync(message.Chat.Id, originMessage.From.Id);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception in Warning : {0} on {1}", e.Message, originMessage.From.Id);
                        }
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

        private async Task ClearWarnsCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to a message >.> baka.",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var table = dbContext.Warns;
                var result = table.FirstOrDefault(x => x.UserId == originMessage.From.Id && x.GroupId == originMessage.Chat.Id);
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
                    result.WarnCount = 0;
                }
            }
            dbContext.SaveChanges();
        }

        private async Task BanCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to a message >.> baka.",
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
                            "I'm not banning senpai! >.<",
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
                        try
                        {
                            await client.KickChatMemberAsync(message.Chat.Id, originUser.Id);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception in Banning : {0} on {1}", e.Message, originMessage.From.Id);
                        }
                    }
                }
            }
        }

        private async Task KickCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to a message >.> baka.",
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
                            "I'm not kicking senpai! >.<",
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

But they can rejoin in a minute UwU",
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        try
                        {
                            await client.KickChatMemberAsync(message.Chat.Id, originUser.Id, System.DateTime.Now.AddMinutes(1));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception in Kicking : {0} on {1}", e.Message, originMessage.From.Id);
                        }
                    }
                }
            }
        }

        private async Task UnbanCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to a message >.> baka.",
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
                try
                {
                    await client.UnbanChatMemberAsync(message.Chat.Id, originUser.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in Unban : {0} on {1}", e.Message, originUser.Id);
                }
            }
        }

        private async Task PinCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to the message you want to pin >.>",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var originUser = originMessage.From;
                await client.PinChatMessageAsync(
                    message.Chat.Id,
                    originMessage.MessageId,
                    true
                );
            }
        }

        private async Task PinLoudlyCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to the message you want to pin >.>",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var originUser = originMessage.From;
                await client.PinChatMessageAsync(
                    message.Chat.Id,
                    originMessage.MessageId
                );
            }
        }

        private async Task SetWelcomeMessageCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to the message you want to pin >.>",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var table = dbContext.GroupMessages;
                var existingEntry = table.Where(welcome => welcome.ChatId == message.Chat.Id).FirstOrDefault();
                if (existingEntry != null)
                {
                    existingEntry.WelcomeMessage = originMessage.MessageId;
                }
                else
                {
                    table.Add(new GroupMessages() { ChatId = message.Chat.Id, WelcomeMessage = originMessage.MessageId });
                }
                dbContext.SaveChanges();
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Set welcome message ^-^",
                    replyToMessageId: message.MessageId
                );
            }
        }

        private async Task SetRulesCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to the message you want to pin >.>",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var table = dbContext.GroupMessages;
                var existingEntry = table.Where(welcome => welcome.ChatId == message.Chat.Id).FirstOrDefault();
                if (existingEntry != null)
                {
                    existingEntry.RulesMessage = originMessage.MessageId;
                }
                else
                {
                    table.Add(new GroupMessages() { ChatId = message.Chat.Id, RulesMessage = originMessage.MessageId });
                }
                dbContext.SaveChanges();
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Set group rules :3",
                    replyToMessageId: message.MessageId
                );
            }
        }

        private async Task SetWelcomeMediaCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Reply to the message you want to pin >.>",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var table = dbContext.GroupMessages;
                var existingEntry = table.Where(welcome => welcome.ChatId == message.Chat.Id).FirstOrDefault();
                if (existingEntry != null)
                {
                    existingEntry.WelcomeMedia = originMessage.MessageId;
                }
                else
                {
                    table.Add(new GroupMessages() { ChatId = message.Chat.Id, WelcomeMedia = originMessage.MessageId });
                }
                dbContext.SaveChanges();
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Set welcome media ^-^",
                    replyToMessageId: message.MessageId
                );
            }
        }

        private async Task ClearWelcomeCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var table = dbContext.GroupMessages;
            var existingEntry = table.Where(welcome => welcome.ChatId == message.Chat.Id).FirstOrDefault();
            if (existingEntry != null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Updated welcome message ^-^",
                    replyToMessageId: message.MessageId
                );
                table.Remove(existingEntry);
            }
            else
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "No welcome message anymore ^-^",
                    replyToMessageId: message.MessageId
                );
            }
            dbContext.SaveChanges();
        }
        private async Task SaveMessage(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            var client = serviceContext.TelegramBotService.Client;
            if (messageParts.Length >= 2)
            {
                var originMessage = message.ReplyToMessage;
                if (originMessage == null)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        "Reply to a message >.> baka.",
                        replyToMessageId: message.MessageId
                    );
                }
                else
                {
                    var table = dbContext.AdminSavedMessages;
                    var existingEntry = table.Where(savedMessage => savedMessage.ChatId == message.Chat.Id && savedMessage.MessageTag == messageParts[1]).FirstOrDefault();
                    if (existingEntry != null)
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
                            "Updated message ^-^",
                            replyToMessageId: message.MessageId
                        );
                        existingEntry.ChatId = message.Chat.Id;
                        existingEntry.MessageId = originMessage.MessageId;
                    }
                    else
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
                            "Saved message ^-^",
                            replyToMessageId: message.MessageId
                        );
                        table.Add(new AdminSavedMessages(messageParts[1], message.Chat.Id, originMessage.MessageId));
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            else
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Invalid request >_<",
                    replyToMessageId: message.MessageId
                );
            }
        }
        private async Task SavedMessage(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            var client = serviceContext.TelegramBotService.Client;
            var table = dbContext.AdminSavedMessages;
            if (messageParts.Length >= 2)
            {
                var tag = messageParts[1];
                var results = table.Where(savedMessage => savedMessage.ChatId == message.Chat.Id && savedMessage.MessageTag == tag);
                if (results.Count() > 0)
                {
                    var result = results.First();
                    try
                    {
                        await client.ForwardMessageAsync
                        (
                            message.Chat.Id,
                            result.ChatId,
                            result.MessageId
                        );
                    }
                    catch (System.Exception e)
                    {
                        System.Console.WriteLine(e);
                    }
                }
                else
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        "No saved messages OwO",
                        replyToMessageId: message.MessageId
                    );
                }
            }
            else
            {
                var results = table.Where(savedMessage => savedMessage.ChatId == message.Chat.Id);
                if (results.Count() > 0)
                {
                    var text = new StringBuilder("<b>Saved Messages</b>\n\n");
                    foreach (var result in results)
                    {
                        text.Append($"<b>{result.MessageTag}</b> : Message in Group {result.ChatId.ToString()}\n");
                    }
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        text.ToString(),
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        "No saved messages OwO",
                        replyToMessageId: message.MessageId
                    );
                }
            }
        }
        private async Task ClearSavedMessage(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            var client = serviceContext.TelegramBotService.Client;
            var table = dbContext.AdminSavedMessages;
            IQueryable<AdminSavedMessages> results;
            if (messageParts.Length >= 2)
            {
                results = table.Where(savedMessage => savedMessage.ChatId == message.Chat.Id && savedMessage.MessageTag == messageParts[1]);
            }
            else
            {
                results = table.Where(savedMessage => savedMessage.ChatId == message.Chat.Id);
            }
            table.RemoveRange(results);
            await dbContext.SaveChangesAsync();
        }
    }
}
