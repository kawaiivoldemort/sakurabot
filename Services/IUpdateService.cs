using System.Threading.Tasks;

using Telegram.Bot.Types;

namespace Sakura.Uwu.Services
{
    public interface IUpdateService
    {
        Task UpdateAsync(Update update);
    }
}