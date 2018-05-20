using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

using Sakura.Uwu.Services;
using Sakura.Uwu.Models;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;

using OpenDotaApi;

namespace Sakura.Uwu.Dota
{
    public delegate Task Command(IBotService botService, Message message, IOpenDotaService dotaService);
    static class DotaCommands
    {
        public static readonly Dictionary<string, Command> Commands = new Dictionary<string, Command>
        {
            { "match", MatchDetailsCommand }
        };

        private static async Task MatchDetailsCommand(IBotService botService, Message message, IOpenDotaService dotaService)
        {
            var client = botService.Client;
            var messageParts = message.Text.Split(' ');
            if(messageParts.Length >= 2)
            {
                var matchIdString = messageParts[2];
                uint matchId;
                var parsed = uint.TryParse(matchIdString, out matchId);
                if(parsed)
                {
                    var match = await dotaService.Client.GetMatchDetails(matchId);
                    if (match != null)
                    {
                        var report = dotaService.Client.DrawMatchReport(match).ToArray();
                        var radiantReports = new List<InputMediaPhoto>();
                        var direReports = new List<InputMediaPhoto>();
                        for(var i = 0; i < report.Length; i++)
                        {
                            var picture = report[i];
                            string pictureName;
                            if(i == 0)
                            {
                                using(var pictureStream = new MemoryStream(picture))
                                {
                                    await client.SendPhotoAsync
                                    (
                                        message.Chat.Id,
                                        new InputOnlineFile(pictureStream, "MatchReport.png"),
                                        disableNotification: true,
                                        caption: "MatchReport",
                                        replyToMessageId: message.MessageId
                                    );
                                }
                            }
                            else if(i < 6)
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
                        await client.SendMediaGroupAsync
                        (
                            message.Chat.Id,
                            radiantReports,
                            disableNotification: true,
                            replyToMessageId: message.MessageId
                        );
                        await client.SendMediaGroupAsync
                        (
                            message.Chat.Id,
                            direReports,
                            disableNotification: true,
                            replyToMessageId: message.MessageId
                        );
                        foreach(var playerReport in radiantReports)
                        {
                            playerReport.Media.Content.Dispose();
                        }
                        foreach(var playerReport in direReports)
                        {
                            playerReport.Media.Content.Dispose();
                        }
                    }
                    else
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
                            "owpen dowta is down >w<!",
                            replyToMessageId: message.MessageId
                        );
                    }                
                }
                else
                {
                    await client.SendTextMessageAsync
                    (
                        message.Chat.Id,
                        "bwad match id >w<!",
                        replyToMessageId: message.MessageId
                    );
                }
            }
            else
            {
                await client.SendTextMessageAsync
                (
                    message.Chat.Id,
                    "pweese giw a walid command UwU!",
                    replyToMessageId: message.MessageId
                );
            }
        }
    }
}