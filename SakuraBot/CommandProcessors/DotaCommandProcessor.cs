using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using OpenDotaApi;

using Sakura.Uwu.Models;
using Sakura.Uwu.Services;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sakura.Uwu.CommandProcessors
{
    class DotaCommandProcessor : ICommandProcessor
    {
        public string Name { get; }
        public UpdateType Type { get; }
        public Dictionary<string, Command> Commands { get; }
        public bool IsFinalCommand { get; }
        public DotaCommandProcessor()
        {
            Name = "Defense of the Ancients 2 (Video Game) Commands";
            Type = UpdateType.Message;
            Commands = new Dictionary<string, Command>
            { { "/dotamatch", new Command() { TaskName = "/dotamatch", TaskDescription = "Get Dota Match Summary", TaskProcess = MatchDetailsCommand } }
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
        public async Task ProcessCommand(Message message, ServicesContext context, BotDbContext dbContext)
        {
            var messageParts = message.Text.Split(' ');
            await this.Commands[messageParts[0]].TaskProcess(message, context, dbContext);
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
        private async Task MatchDetailsCommand(Message message, ServicesContext context, BotDbContext dbContext)
        {
            var client = context.TelegramBotService.Client;
            var messageParts = message.Text.Split(' ');
            if (messageParts.Length > 1)
            {
                var matchIdString = messageParts[1];
                uint matchId;
                var parsed = uint.TryParse(matchIdString, out matchId);
                if (parsed)
                {
                    var match = await context.DotaService.Client.GetMatchDetails(matchId);
                    if (match != null)
                    {
                        var report = context.DotaService.Client.DrawMatchReport(match).ToArray();
                        var radiantReports = new List<InputMediaPhoto>();
                        var direReports = new List<InputMediaPhoto>();
                        for (var i = 0; i < report.Length; i++)
                        {
                            var picture = report[i];
                            string pictureName;
                            if (i == 0)
                            {
                                using(var pictureStream = new MemoryStream(picture))
                                {
                                    await client.SendPhotoAsync(
                                        message.Chat.Id,
                                        new InputOnlineFile(pictureStream, "MatchReport.png"),
                                        disableNotification : true,
                                        caption: "MatchReport",
                                        replyToMessageId : message.MessageId
                                    );
                                }
                            }
                            else if (i < 6)
                            {
                                pictureName = $"{((i - 1) / 5 == 0 ? "Radiant" : "Dire")} {(i - 1) % 5 + 1}";
                                var media = new InputMediaPhoto();
                                var pictureStream = new MemoryStream(picture);
                                media.Media = new InputMedia(pictureStream, $"{pictureName}.png");
                                media.Caption = pictureName;
                                radiantReports.Add(media);
                            }
                            else
                            {
                                pictureName = $"{((i - 1) / 5 == 0 ? "Radiant" : "Dire")} {(i - 1) % 5 + 1}";
                                var media = new InputMediaPhoto();
                                var pictureStream = new MemoryStream(picture);
                                media.Media = new InputMedia(pictureStream, $"{pictureName}.png");
                                media.Caption = pictureName;
                                direReports.Add(media);
                            }
                        }
                        await client.SendMediaGroupAsync(
                            message.Chat.Id,
                            radiantReports,
                            disableNotification : true,
                            replyToMessageId : message.MessageId
                        );
                        await client.SendMediaGroupAsync(
                            message.Chat.Id,
                            direReports,
                            disableNotification : true,
                            replyToMessageId : message.MessageId
                        );
                        foreach (var playerReport in radiantReports)
                        {
                            playerReport.Media.Content.Dispose();
                        }
                        foreach (var playerReport in direReports)
                        {
                            playerReport.Media.Content.Dispose();
                        }
                    }
                    else
                    {
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            "owpen dowta is down >w<!",
                            replyToMessageId : message.MessageId
                        );
                    }
                }
                else
                {
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        "bwad match id >w<!",
                        replyToMessageId : message.MessageId
                    );
                }
            }
            else
            {
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    "pweese giw a walid command UwU!",
                    replyToMessageId : message.MessageId
                );
            }
        }
    }
}
