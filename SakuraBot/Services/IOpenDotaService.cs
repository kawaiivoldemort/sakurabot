using OpenDotaApi;

namespace Sakura.Uwu.Services
{
    public interface IOpenDotaService
    {
        DotaClient Client { get; }
    }
}