using System.Collections.Generic;

using Sakura.Uwu.Models;
using Sakura.Uwu.CommandProcessors;

namespace Sakura.Uwu.Services
{
    public class CommandProcessorService : ICommandProcessorService
    {
        public CommandProcessorService(IBotService botService, IOpenDotaService dotaService)
        {
            ServiceContext = new ServicesContext((BotService) botService, (OpenDotaService) dotaService);            
            CommandProcessors = new List<ICommandProcessor>();
            CommandProcessors.Add(new TrackingCommandProcessor());
            CommandProcessors.Add(new GroupWelcomeCommandProcessor());
            CommandProcessors.Add(new UserCommandProcessor());
            CommandProcessors.Add(new DotaCommandProcessor());
            CommandProcessors.Add(new AdminCommandProcessor());
        }
        public ServicesContext ServiceContext { get; }
        public List<ICommandProcessor> CommandProcessors { get; }
    }
}