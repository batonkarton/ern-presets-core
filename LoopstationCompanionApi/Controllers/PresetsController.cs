using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoopstationCompanionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresetsController : ControllerBase
    {

        private static readonly List<Preset> _presets =
        [
            new() { Name = "Default Vocal", DeviceModel = "RC-505mkII"},
            new() { Name = "Beatbox Chain", DeviceModel = "RC-505mkII"}
        ];

        /// <summary>
        /// GET /api/presets
        /// Returns a list of all presets (from in-memory store for now).
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<PresetListItemDto>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var items = _presets
                .OrderByDescending(p => p.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PresetListItemDto(p.Id, p.Name, p.DeviceModel, p.UpdatedAt))
                .ToList();

            return Ok(items);
        }
    }
}
