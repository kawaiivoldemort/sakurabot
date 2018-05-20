using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
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
            { "/whoisthis", WhoIsThisCommand },
            { "/admins", ListAdminsCommand },
            { "/whatmedia", WhatMediaCommand },
            { "/wut", WutCommand },
            { "/gurl", GurlCommand },
            { "/stahp", StahpCommand },
            { "/tellmewhy", TellMeWhyCommand },
            { "/save", SaveMessage },
            { "/saved", SavedMessage },
            { "/clearsaved", ClearSavedMessage }
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
        private static async Task WhoIsThisCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
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

        private static async Task WhatMediaCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
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

        private static InputOnlineFile wutFile = new InputOnlineFile("CgADBAAD3ZQAAiQaZAfJNSjIa8ybFwI");
        private static async Task WutCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
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
        private static async Task GurlCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
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
        private static async Task StahpCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
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
        private static async Task TellMeWhyCommand(IBotService botService, Message message, BotDbContext dbContext)
        {
            var client = botService.Client;
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
        
        private static async Task SaveMessage(IBotService botService, Message message, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            var client = botService.Client;
            if(messageParts.Length >= 2)
            {                
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
                    var table = dbContext.SavedMessages;
                    var existingEntry = table.Where(savedMessage => savedMessage.UserId == message.From.Id && savedMessage.MessageTag == messageParts[1]).FirstOrDefault();
                    if(existingEntry != null)
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
                            "Updated Message",
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
                            "Saved Message",
                            replyToMessageId: message.MessageId
                        );
                        table.Add(new UserSavedMessages(messageParts[1], message.From.Id, message.Chat.Id, originMessage.MessageId));
                    }
                    dbContext.SaveChanges();
                }
            }
            else
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "Invalid Request",
                    replyToMessageId: message.MessageId
                );
            }
        }
        
        private static async Task SavedMessage(IBotService botService, Message message, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            var client = botService.Client;
            var table = dbContext.SavedMessages;
            if(messageParts.Length >= 2)
            {
                var tag = messageParts[1];
                var results = table.Where(savedMessage => savedMessage.UserId == message.From.Id && savedMessage.MessageTag == tag);
                if(results.Count() > 0)
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
                        "No Saved Messages",
                        replyToMessageId: message.MessageId
                    );
                }
            }
            else
            {
                var results = table.Where(savedMessage => savedMessage.UserId == message.From.Id);
                if(results.Count() > 0)
                {
                    var text = new StringBuilder("<b>Saved Messages</b>\n\n");
                    foreach(var result in results)
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
                        "No Saved Messages",
                        replyToMessageId: message.MessageId
                    );
                }
            }
        }

        
        private static async Task ClearSavedMessage(IBotService botService, Message message, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            var client = botService.Client;
            var table = dbContext.SavedMessages;
            IQueryable<UserSavedMessages> results;
            if(messageParts.Length >= 2)
            {
                results = table.Where(savedMessage => savedMessage.UserId == message.From.Id && savedMessage.MessageTag == messageParts[1]);
            }
            else
            {
                results = table.Where(savedMessage => savedMessage.UserId == message.From.Id);
            }
            table.RemoveRange(results);
            dbContext.SaveChanges();
        }
    }
}