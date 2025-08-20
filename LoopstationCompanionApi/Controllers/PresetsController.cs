using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoopstationCompanionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // -> /api/presets
    public class PresetsController : ControllerBase
    {
        private static readonly List<Preset> _presets =
        [
            new() { Name = "Default Vocal", DeviceModel = DeviceModel.RC505MkII, JsonModel = "{\"deviceModel\":\"RC-505mkII\",\"inputFx\":{},\"trackFx\":{}}"},
            new() { Name = "Beatbox Chain", DeviceModel = DeviceModel.RC505MkII, JsonModel = "{\"deviceModel\":\"RC-505mkII\",\"inputFx\":{},\"trackFx\":{}}"}
        ];

        // GET /api/presets?page=1&pageSize=20
        [HttpGet]
        public ActionResult<IEnumerable<PresetListItemDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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

        // GET /api/presets/{id}
        [HttpGet("{id:guid}")]
        public ActionResult<PresetDetailDto> GetById(Guid id)
        {
            var p = _presets.FirstOrDefault(x => x.Id == id);
            if (p is null) return NotFound();

            var dto = new PresetDetailDto(p.Id, p.Name, p.DeviceModel, p.UpdatedAt, p.JsonModel);
            return Ok(dto);
        }

        // POST /api/presets
        [HttpPost]
        public ActionResult Create([FromBody] CreatePresetRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { error = "Name is required." });

            var defaultModel = "{\"deviceModel\":\"RC-505mkII\",\"inputFx\":{},\"trackFx\":{}}";

            var p = new Preset
            {
                Name = req.Name.Trim(),
                DeviceModel = req.DeviceModel,
                JsonModel = defaultModel,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _presets.Add(p);
            return CreatedAtAction(nameof(GetById), new { id = p.Id }, new { id = p.Id, name = p.Name });
        }

        // PUT /api/presets/{id}
        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, [FromBody] UpdatePresetRequest req)
        {
            var p = _presets.FirstOrDefault(x => x.Id == id);
            if (p is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(req.Name))
                p.Name = req.Name.Trim();

            if (req.DeviceModel.HasValue)
                p.DeviceModel = req.DeviceModel.Value;

            p.UpdatedAt = DateTime.UtcNow;
            return NoContent();
        }

        // DELETE /api/presets/{id}
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var idx = _presets.FindIndex(x => x.Id == id);
            if (idx < 0) return NotFound();

            _presets.RemoveAt(idx);
            return NoContent();
        }
    }
}
