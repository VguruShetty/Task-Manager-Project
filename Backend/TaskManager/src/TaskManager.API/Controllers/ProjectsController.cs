using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Fetch all projects with optional filter by owner ID.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetAll(
            [FromQuery] Guid? ownerId,
            CancellationToken ct)
        {
            var projects = await _projectService.GetAllAsync(ownerId, ct);
            return Ok(projects);
        }

        /// <summary>
        /// Fetch a single project along with its tasks and owner.
        /// </summary>
        [HttpGet("{id:guid}", Name = nameof(GetProjectById))]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectResponseDto>> GetProjectById(Guid id, CancellationToken ct)
        {
            var project = await _projectService.GetByIdAsync(id, ct);
            if (project is null)
                return NotFound(new { message = $"Project with ID '{id}' was not found." });

            return Ok(project);
        }

        /// <summary>
        /// Create a new project.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectResponseDto>> Create(
            [FromBody] CreateProjectDto dto,
            CancellationToken ct)
        {
            try
            {
                var created = await _projectService.CreateAsync(dto, ct);
                return CreatedAtRoute(nameof(GetProjectById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update project name or description.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProjectDto dto,
            CancellationToken ct)
        {
            var updated = await _projectService.UpdateAsync(id, dto, ct);
            if (!updated)
                return NotFound(new { message = $"Project with ID '{id}' was not found." });

            return NoContent();
        }

        /// <summary>
        /// Delete a project and its associated tasks.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var deleted = await _projectService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(new { message = $"Project with ID '{id}' was not found." });

            return NoContent();
        }
    }
}
