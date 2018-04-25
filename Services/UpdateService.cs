using System.Threading.Tasks;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Sakura.Uwu.Models;
using Sakura.Uwu.GroupManagement;
using static Sakura.Uwu.Common.Accessories;

namespace Sakura.Uwu.Services 
{
    // Service to respond to API requests
    public class UpdateService : IUpdateService
    {
        private readonly BotDbContext _dbContext;
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(BotDbContext dbContext, IBotService botService, ILogger<UpdateService> logger)
        {
            _dbContext = dbContext;
            _botService = botService;
            _logger = logger;
        }

        public async Task UpdateAsync(Update update)
        {
            Task commandTask = null;
            Task trackTask = null;
            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {
                var message = update.Message;
                trackTask = Task.Factory.StartNew(() => Commands.Track(_botService, message, _dbContext));
                if
                (
                    (message.Entities != null) &&
                    (message.Entities[0].Offset == 0) &&
                    (message.Entities[0].Type == MessageEntityType.BotCommand)
                )
                {
                    var commandMetadata = message.Entities[0];
                    var command = message.Text.Substring(commandMetadata.Offset, commandMetadata.Length);
                    var chat = await _botService.Client.GetChatAsync(message.Chat.Id);
                    if (chat.Type == ChatType.Group || chat.Type == ChatType.Supergroup)
                    {
                        if (Commands.User.ContainsKey(command))
                        {
                            commandTask = Commands.User[command](_botService, message, _dbContext);
                        } else {
                            var admins = await _botService.Client.GetChatAdministratorsAsync(message.Chat.Id);
                            if (admins.Any(admin => admin.User.Id == message.From.Id))
                            {
                                if (Commands.Admin.ContainsKey(command))
                                {
                                    commandTask = Commands.Admin[command](_botService, message, _dbContext);
                                }                      
                            }
                            else
                            {
                                await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Naughty senpai, you aren't admin", replyToMessageId: message.MessageId);                            
                            }
                        }
                    }
                }
            }
            if (commandTask != null)
            {
                commandTask.Wait();
            }
            if (trackTask != null)
            {
                trackTask.Wait();
            }
        }
    }
}