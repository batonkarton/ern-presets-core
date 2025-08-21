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

        // GET /api/presets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Preset>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var items = await _service.GetAllAsync(page, pageSize);
            return Ok(items);
        }

        // GET /api/presets/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Preset>> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // POST /api/presets
        [HttpPost]
        public async Task<ActionResult<Preset>> Create([FromBody] CreatePresetRequest body)
        {
            if (string.IsNullOrWhiteSpace(body.Name))
                return BadRequest("Name is required.");

            var created = await _service.CreateAsync(new Preset
            {
                Name = body.Name,
                DeviceModel = body.DeviceModel
            });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/presets
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Preset>> Update(Guid id, [FromBody] UpdatePresetRequest body)
        {
            if (string.IsNullOrWhiteSpace(body.Name))
                return BadRequest("Name is required.");

            var updated = await _service.UpdateAsync(id, new Preset
            {
                Name = body.Name,
                DeviceModel = body.DeviceModel
            });

            return updated is null ? NotFound() : Ok(updated);
        }

        // DELETE /api/presets/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var removed = await _service.DeleteAsync(id);
            return removed ? NoContent() : NotFound();
        }
    }
}
