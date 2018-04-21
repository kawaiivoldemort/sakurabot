using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sakura.Uwu.Services {
    // Service to respond to API requests
    public class UpdateService : IUpdateService {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IBotService botService, ILogger<UpdateService> logger) {
            _botService = botService;
            _logger = logger;
        }

        public async Task UpdateAsync(Update update) {
            var message = update.Message;
            if(message.Type == MessageType.Text) {
                await _botService.Client.SendTextMessageAsync(message.Chat.Id, message.Text);
            }
        }
    }
}