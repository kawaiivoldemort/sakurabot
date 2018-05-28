using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Sakura.Uwu.Models;
using Sakura.Uwu.CommandProcessors;
using static Sakura.Uwu.Common.Accessories;

namespace Sakura.Uwu.Services 
{
    // Service to respond to API requests
    public class UpdateService : IUpdateService
    {
        private readonly CommandProcessorService _commandProcessorService;
        private readonly ILogger<UpdateService> _logger;
        private readonly BotDbContext _dbContext;

        public UpdateService(BotDbContext dbContext, ICommandProcessorService commandProcessorService, ILogger<UpdateService> logger)
        {
            _commandProcessorService = (CommandProcessorService) commandProcessorService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task UpdateAsync(Update update)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    if(update.Message.Type == MessageType.Text && update.Message.Text.StartsWith("/help"))
                    {
                        await _commandProcessorService.ServiceContext.TelegramBotService.Client.SendTextMessageAsync
                        (
                            update.Message.Chat.Id,
                            $"<b>Sakura Bot</b>\n\n{string.Join("", _commandProcessorService.CommandProcessors.Select(cp => cp.GetDescriptions()))}--------",
                            parseMode: ParseMode.Html
                        );
                    }
                }
                var commandTaskList = new List<Task>();
                foreach(var commandProcessor in _commandProcessorService.CommandProcessors)
                {
                    if (update.Type == commandProcessor.Type)
                    if(commandProcessor.DoesProcessCommand(update))
                    {
                        commandTaskList.Add
                        (
                            commandProcessor.ProcessCommand
                            (
                                update.Message,
                                _commandProcessorService.ServiceContext,
                                _dbContext
                            )
                        );
                        if(commandProcessor.IsFinalCommand)
                        {
                            break;
                        }
                    }
                }
                await Task.WhenAll(commandTaskList.ToArray());
            }
            catch(Exception e)
            {
                System.Console.WriteLine($@"Exception Ocurred: {e}");
            }
        }
    }
}