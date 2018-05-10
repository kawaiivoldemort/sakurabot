using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Telegram.Bot.Types;

using Sakura.Uwu.Services;

namespace Sakura.Uwu.Controllers
{
    // Request Controllers
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly IUpdateService _updateService;

        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        // POST api/update
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            await _updateService.UpdateAsync(update);
            return Ok();
        }
    }
}