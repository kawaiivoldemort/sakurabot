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
                var profilePhotos = await client.GetUserProfilePhotosAsync(newMember.Id, 0, 1);
                if (profilePhotos.TotalCount != 0)
                {
                    await client.SendPhotoAsync
                    (
                        message.Chat.Id,
                        profilePhotos.Photos[0][0].FileId,
$@"Welcome
<b>{newMember.FirstName} {newMember.LastName}{(newMember.IsBot ? "🤖" : "")}</b>
@{newMember.Username}
<code>{newMember.Id}</code>",
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
<code>{newMember.Id}</code>",
                        parseMode: ParseMode.Html
                    );
                    
                }
            }
        }
    }
}