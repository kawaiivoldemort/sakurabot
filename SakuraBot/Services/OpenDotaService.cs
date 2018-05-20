using Microsoft.Extensions.Options;

using OpenDotaApi;

using Sakura.Uwu.Models;

namespace Sakura.Uwu.Services
{
    // OpenDota service to handle OpenDota Requests
    public class OpenDotaService : IOpenDotaService
    {
        public OpenDotaService(IOptions<BotSettings> botSettings)
        {
            this.Client = new DotaClient(false);
        }
        public DotaClient Client { get; }
    }
}