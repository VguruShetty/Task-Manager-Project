using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[Authorize]
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
    /// Search, sort, and paginate the current user's projects.
    /// </summary>
    /// <remarks>
    /// Example: GET /api/projects?pageNumber=1&amp;pageSize=10&amp;sortBy=name&amp;isDescending=false&amp;searchTerm=backend
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProjectResponseDto>>> GetAll(
        [FromQuery] ProjectQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await _projectService.GetProjectsAsync(parameters, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a project by its unique ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetProjectById))]
    [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponseDto>> GetProjectById(Guid id, CancellationToken ct)
    {
        var project = await _projectService.GetProjectByIdAsync(id, ct);
        if (project is null)
        {
            return NotFound(new { message = $"Project with ID '{id}' was not found." });
        }

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
        var created = await _projectService.CreateProjectAsync(dto, ct);
        return CreatedAtRoute(nameof(GetProjectById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update a project's name or description.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectDto dto,
        CancellationToken ct)
    {
        var updated = await _projectService.UpdateProjectAsync(id, dto, ct);
        if (!updated)
        {
            return NotFound(new { message = $"Project with ID '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a project and cascade-remove its associated tasks.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _projectService.DeleteProjectAsync(id, ct);
        if (!deleted)
        {
            return NotFound(new { message = $"Project with ID '{id}' was not found." });
        }

        return NoContent();
    }
}