using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoopstationCompanionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresetsController : ControllerBase
    {
        private readonly IPresetService _service;
        public PresetsController(IPresetService service) => _service = service;

        /// GET /api/presets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Preset>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var items = await _service.GetAllAsync(page, pageSize);
            return Ok(items);
        }
    }
}
