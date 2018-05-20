using System;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Sakura.Uwu.Services;
using Sakura.Uwu.Models;

namespace Sakura.Uwu.GroupManagement
{
    partial class Responses
    {
        public static async Task WelcomeResponse(IBotService botService, Message message, BotDbContext dbContext)
        {
            var newMembers = message.NewChatMembers;
            var client = botService.Client;
            foreach (var newMember in newMembers)
            {
                try
                {
                    var profilePhotos = await client.GetUserProfilePhotosAsync(newMember.Id, 0, 1);
                    var table = dbContext.WelcomeMessages;
                    var setWelcomeFooter = table.Where(welcome => welcome.ChatId == message.Chat.Id).FirstOrDefault();
                    var welcomeFooter = (setWelcomeFooter != null ? $"\n\n{setWelcomeFooter.Text}" : "");
                    if (profilePhotos.TotalCount != 0)
                    {
                        await client.SendPhotoAsync
                        (
                            message.Chat.Id,
                            profilePhotos.Photos[0][0].FileId,
$@"Welcome

<b>{newMember.FirstName} {newMember.LastName}{(newMember.IsBot ? "🤖" : "")}</b>
@{newMember.Username}
<code>{newMember.Id}</code>" + welcomeFooter,
                            parseMode: ParseMode.Html
                        );
                    }
                    else
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
$@"Welcome

<b>{newMember.FirstName} {newMember.LastName}{(newMember.IsBot ? "🤖" : "")}</b>
@{newMember.Username}
<code>{newMember.Id}</code>" + welcomeFooter,
                            parseMode: ParseMode.Html
                        );
                        
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in Welcome : {0}", e.Message);
                }
            }
        }
    }
}