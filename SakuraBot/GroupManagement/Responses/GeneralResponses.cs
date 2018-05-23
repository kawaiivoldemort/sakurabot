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
                    var setWelcomeText = table.Where(welcome => welcome.ChatId == message.Chat.Id).FirstOrDefault();
                    var welcomeMessage = $@"Welcome

<b>{newMember.FirstName} {newMember.LastName}{(newMember.IsBot ? "🤖" : "")}</b>
@{newMember.Username}
<code>{newMember.Id}</code>

{(setWelcomeText != null ? setWelcomeText.Text : null)}";
                    if (profilePhotos.TotalCount != 0)
                    {
                        await client.SendPhotoAsync
                        (
                            message.Chat.Id,
                            profilePhotos.Photos[0][0].FileId,
                            welcomeMessage.Substring(0, 200),
                            parseMode: ParseMode.Html
                        );
                        if(welcomeMessage.Length > 200)
                        {
                            await client.SendTextMessageAsync
                            (
                                message.Chat.Id,
                                welcomeMessage.Substring(200),
                                parseMode: ParseMode.Html
                            );
                        }
                    }
                    else
                    {
                        await client.SendTextMessageAsync
                        (
                            message.Chat.Id,
                            welcomeMessage,
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