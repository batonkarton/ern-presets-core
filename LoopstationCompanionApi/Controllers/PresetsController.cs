using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoopstationCompanionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresetsController : ControllerBase
    {
        private readonly IPresetService _presetService;
        public PresetsController(IPresetService service) => _presetService = service;

        // GET /api/presets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PresetSummary>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var items = await _presetService.GetAllAsync(page, pageSize);
            return Ok(items);
        }

        // GET /api/presets/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Preset>> GetById(Guid id)
        {
            var item = await _presetService.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // POST /api/presets
        [HttpPost]
        public async Task<ActionResult<Preset>> Create([FromBody] CreatePresetRequest body)
        {
            if (string.IsNullOrWhiteSpace(body.Name))
                return BadRequest("Name is required.");

            var created = await _presetService.CreateAsync(new Preset
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

            var updated = await _presetService.UpdateAsync(id, new Preset
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
            var removed = await _presetService.DeleteAsync(id);
            return removed ? NoContent() : NotFound();
        }

        // POST /api/presets/{id}/import
        [HttpPost("{id:guid}/import")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Preset>> ImportRc0(
            Guid id,
            [FromForm] ImportRc0Request? request,
            CancellationToken ct)
        {
            if (request is null || request.File is null)
                return BadRequest("File is required.");

            var file = request.File;

            if (!file.FileName.EndsWith(".rc0", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .rc0 files are supported.");

            try
            {
                var updated = await _presetService.ImportRc0Async(id, request.File, ct);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
