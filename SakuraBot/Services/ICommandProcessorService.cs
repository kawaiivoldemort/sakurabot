using System.Collections.Generic;

using Sakura.Uwu.CommandProcessors;
using Sakura.Uwu.Models;

namespace Sakura.Uwu.Services
{
    public class ServicesContext
    {
        public BotService TelegramBotService { get; }
        public OpenDotaService DotaService { get; }
        public ServicesContext
        (
            BotService botService,
            OpenDotaService dotaService
        )
        {
            TelegramBotService = botService;
            DotaService = dotaService;
        }
    }
    public interface ICommandProcessorService
    {
        ServicesContext ServiceContext { get; }
        List<ICommandProcessor> CommandProcessors { get; }
    }
}