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
    class UserCommandProcessor : ICommandProcessor
    {
        public string Name { get; }
        public UpdateType Type { get; }
        public Dictionary<string, Command> Commands { get; }
        public bool IsFinalCommand { get; }
        public UserCommandProcessor()
        {
            Name = "User Commands";
            Type = UpdateType.Message;
            Commands = new Dictionary<string, Command>
            {
                { "/whoami",        new Command() { TaskName="/whoami",        TaskDescription="Get User Summary (Self)",           TaskProcess=WhoAmICommand       } },
                { "/whoisthis",     new Command() { TaskName="/whoisthis",     TaskDescription="Get User Summary (Other)",          TaskProcess=WhoIsThisCommand    } },
                { "/admins",        new Command() { TaskName="/admins",        TaskDescription="Get Group Admins",                  TaskProcess=ListAdminsCommand   } },
                { "/whatmedia",     new Command() { TaskName="/whatmedia",     TaskDescription="Get Media Message Summary",         TaskProcess=WhatMediaCommand    } },
                { "/rules",         new Command() { TaskName="/rules",         TaskDescription="Get Group Rules",                   TaskProcess=RulesCommand        } },
                { "/wut",           new Command() { TaskName="/wut",           TaskDescription="Get Dota Match Summary",            TaskProcess=WutCommand          } },
                { "/gurl",          new Command() { TaskName="/gurl",          TaskDescription="Get Dota Match Summary",            TaskProcess=GurlCommand         } },
                { "/stahp",         new Command() { TaskName="/stahp",         TaskDescription="Get Dota Match Summary",            TaskProcess=StahpCommand        } },
                { "/tellmewhy",     new Command() { TaskName="/tellmewhy",     TaskDescription="Get Dota Match Summary",            TaskProcess=TellMeWhyCommand    } },
            };
            IsFinalCommand = true;
        }
        public bool DoesProcessCommand(Update update)
        {
            if(update.Message.Type == MessageType.Text)
            {
                var messageParts = update.Message.Text.Split(' ');
                if(this.Commands.ContainsKey(messageParts[0]))
                {
                    return true;
                }
            }
            return false;
        }
        public async Task ProcessCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            await this.Commands[messageParts[0]].TaskProcess(message, serviceContext, dbContext);
        }
        public string GetDescriptions()
        {
            var descriptions = new StringBuilder($"--------\n<b>{Name}</b>\n\n");
            foreach(var cmd in this.Commands)
            {
                descriptions.AppendLine($"<b>{cmd.Value.TaskName}</b>: <i>{cmd.Value.TaskDescription}</i>");
            }
            return descriptions.ToString();
        }
        private async Task StartCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
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
        private async Task WhoAmICommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
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
                    replyToMessageId: message.MessageId,
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
                    replyToMessageId: message.MessageId,
                    parseMode: ParseMode.Html
                );
                
            }
        }
        private async Task WhoIsThisCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese repwy to that users message fiwst >w<!",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                var profilePhotos = await client.GetUserProfilePhotosAsync(originMessage.From.Id, 0, 1);
                if (profilePhotos.TotalCount != 0)
                {
                    await client.SendPhotoAsync
                    (
                        message.Chat.Id,
                        profilePhotos.Photos[0][0].FileId,
$@"This is
<b>{originMessage.From.FirstName} {originMessage.From.LastName}{(originMessage.From.IsBot ? "🤖" : "")}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"You are
<b>{originMessage.From.FirstName} {originMessage.From.LastName}{(originMessage.From.IsBot ? "🤖" : "")}</b>
@{originMessage.From.Username}
<code>{originMessage.From.Id}</code>",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
            }
        }
        private async Task ListAdminsCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
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
        private async Task WhatMediaCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese repwy to that users message fiwst >w<!",
                    replyToMessageId: message.MessageId
                );
            }
            else
            {
                if(originMessage.Type == MessageType.Audio)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Audio

{originMessage.Audio.Title} - {originMessage.Audio.Performer}
Duration : {originMessage.Audio.Duration}

<b>{originMessage.Audio.FileId}</b>

Filesize : {originMessage.Audio.FileSize}
Poster : @{originMessage.From.Username}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else if(originMessage.Type == MessageType.Photo)
                {
                    var photo = originMessage.Photo.OrderByDescending(photoSize => photoSize.Height).First();
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Photo

Dimensions : {photo.Width}x{photo.Height}

<b>{photo.FileId}</b>

Filesize : {photo.FileSize}
Poster : @{originMessage.From.Username}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else if(originMessage.Type == MessageType.Video)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Video

Dimensions : {originMessage.Video.Width}x{originMessage.Video.Height}
Duration : {originMessage.Video.Duration}

<b>{originMessage.Video.FileId}</b>

Filesize : {originMessage.Video.FileSize}
Poster : @{originMessage.From.Username}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else if(originMessage.Type == MessageType.Voice)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Voice Message

Duration : {originMessage.Voice.Duration}

<b>{originMessage.Voice.FileId}</b>

Filesize : {originMessage.Voice.FileSize}
Poster : @{originMessage.From.Username}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else if(originMessage.Type == MessageType.VideoNote)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"VideoNote

Duration : {originMessage.VideoNote.Duration}

<b>{originMessage.VideoNote.FileId}</b>

Filesize : {originMessage.VideoNote.FileSize}
Poster : @{originMessage.From.Username}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else if(originMessage.Type == MessageType.Sticker)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Sticker

Dimensions : {originMessage.Sticker.Width}x{originMessage.Sticker.Height}
Sticker Pack : {originMessage.Sticker.SetName}
Emoji : {originMessage.Sticker.Emoji}

<b>{originMessage.Sticker.FileId}</b>

Filesize : {originMessage.Sticker.FileSize}
Poster : @{originMessage.From.Username}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else if(originMessage.Type == MessageType.Document)
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
$@"Document {originMessage.Document.FileName}

<b>{originMessage.Document.FileId}</b>

Filesize : {originMessage.Document.FileSize}
Poster : @{originMessage.From.Username}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
                else
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        $"No Media\nMessage Type : {originMessage.Type.ToString()}",
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                }
            }
        }
        private async Task RulesCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var table = dbContext.GroupMessages;
            var existingEntry = table.Where(welcome => welcome.ChatId == message.Chat.Id).FirstOrDefault();
            if(existingEntry != null)
            {
                await client.ForwardMessageAsync
                (
                    message.Chat.Id,
                    message.Chat.Id,
                    (int) existingEntry.RulesMessage
                );
            }
            else
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "No Rules Set",
                    replyToMessageId: message.MessageId
                );
            }
        }
        private static InputOnlineFile wutFile = new InputOnlineFile("CgADBAAD3ZQAAiQaZAfJNSjIa8ybFwI");
        private async Task WutCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendDocumentAsync
                (
                    message.Chat.Id,
                    wutFile
                );
            }
            else
            {
                await client.SendDocumentAsync
                (
                    message.Chat.Id,
                    wutFile,
                    replyToMessageId: originMessage.MessageId
                );
            }
        }
        private static InputOnlineFile gurlFile = new InputOnlineFile("CgADBAAD-J4AAr0XZAfsBBNcYH1u5gI");
        private async Task GurlCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendDocumentAsync
                (
                    message.Chat.Id,
                    gurlFile
                );
            }
            else
            {
                await client.SendDocumentAsync
                (
                    message.Chat.Id,
                    gurlFile,
                    replyToMessageId: originMessage.MessageId
                );
            }
        }

        private static InputOnlineFile stahpFile = new InputOnlineFile("CgADBAADgqAAAiQdZAcIQd39ISh6SQI");
        private async Task StahpCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendDocumentAsync
                (
                    message.Chat.Id,
                    stahpFile
                );
            }
            else
            {
                await client.SendDocumentAsync
                (
                    message.Chat.Id,
                    stahpFile,
                    replyToMessageId: originMessage.MessageId
                );
            }
        }

        private static InputOnlineFile tellMeWhyFile = new InputOnlineFile("BQADBQADTwADBWcJVFRdrXnZKD7IAg");
        private async Task TellMeWhyCommand(Message message, ServicesContext serviceContext, BotDbContext dbContext)
        {
            var client = serviceContext.TelegramBotService.Client;
            var originMessage = message.ReplyToMessage;
            if (originMessage == null)
            {
                await client.SendAudioAsync
                (
                    message.Chat.Id,
                    tellMeWhyFile
                );
            }
            else
            {
                await client.SendAudioAsync
                (
                    message.Chat.Id,
                    tellMeWhyFile,
                    replyToMessageId: message.MessageId
                );
            }
        }
    }
}