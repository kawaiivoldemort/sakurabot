using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Sakura.Uwu.Models;
using Sakura.Uwu.Services;

namespace Sakura.Uwu.CommandProcessors
{
    class GroupWelcomeCommandProcessor : ICommandProcessor
    {
        public string Name { get; }
        public UpdateType Type { get; }
        public Dictionary<string, Command> Commands { get; }
        public bool IsFinalCommand { get; }
        public GroupWelcomeCommandProcessor()
        {
            Name = "Group Welcome Process";
            Type = UpdateType.Message;
            IsFinalCommand = true;
        }
        
        public bool DoesProcessCommand(Update update)
        {
            if(update.Message.Type == MessageType.ChatMembersAdded)
            {
                return true;
            }
            return false;
        }
        public async Task ProcessCommand(Message message, ServicesContext context, BotDbContext dbContext)
        {
            var newMembers = message.NewChatMembers;
            var client = context.TelegramBotService.Client;
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
        public string GetDescriptions()
        {
            return "";
        }
    }
}