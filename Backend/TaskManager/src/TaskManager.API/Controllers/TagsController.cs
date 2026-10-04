using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Tags;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Retrieve all available tags.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TagDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAll(CancellationToken ct)
        {
            var tags = await _tagService.GetAllAsync(ct);
            return Ok(tags);
        }

        /// <summary>
        /// Retrieve a specific tag by ID.
        /// </summary>
        [HttpGet("{id:guid}", Name = nameof(GetTagById))]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagDto>> GetTagById(Guid id, CancellationToken ct)
        {
            var tag = await _tagService.GetByIdAsync(id, ct);
            if (tag is null)
            {
                return NotFound(new { message = $"Tag with ID '{id}' was not found." });
            }

            return Ok(tag);
        }

        /// <summary>
        /// Create a new tag with custom label and hex color.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagDto dto, CancellationToken ct)
        {
            var created = await _tagService.CreateAsync(dto, ct);
            return CreatedAtRoute(nameof(GetTagById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing tag's name or color.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagDto dto, CancellationToken ct)
        {
            var updated = await _tagService.UpdateAsync(id, dto, ct);
            if (!updated)
            {
                return NotFound(new { message = $"Tag with ID '{id}' was not found." });
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a tag by ID.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var deleted = await _tagService.DeleteAsync(id, ct);
            if (!deleted)
            {
                return NotFound(new { message = $"Tag with ID '{id}' was not found." });
            }

            return NoContent();
        }
    }
}
