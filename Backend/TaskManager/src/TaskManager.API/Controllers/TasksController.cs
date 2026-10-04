using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Search, filter, sort, and paginate tasks.
    /// </summary>
    /// <remarks>
    /// Example: GET /api/tasks?pageNumber=1&amp;pageSize=15&amp;status=1&amp;sortBy=duedate&amp;isDescending=false&amp;searchTerm=bug
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TaskItemResponseDto>>> GetAll(
        [FromQuery] TaskQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await _taskService.GetTasksAsync(parameters, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific task by its unique ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(TaskItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        var task = await _taskService.GetTaskByIdAsync(id, ct);
        if (task is null)
        {
            return NotFound(new { message = $"Task with ID '{id}' was not found." });
        }

        return Ok(task);
    }

    /// <summary>
    /// Create a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskItemResponseDto>> Create(
        [FromBody] CreateTaskItemDto dto,
        CancellationToken ct)
    {
        var created = await _taskService.CreateTaskAsync(dto, ct);
        return CreatedAtRoute(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update all fields of an existing task.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskItemDto dto,
        CancellationToken ct)
    {
        var updated = await _taskService.UpdateTaskAsync(id, dto, ct);
        if (!updated)
        {
            return NotFound(new { message = $"Task with ID '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Partially update only the task status (ideal for Kanban drag-and-drop).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusDto dto,
        CancellationToken ct)
    {
        var updated = await _taskService.UpdateStatusAsync(id, dto.Status, ct);
        if (!updated)
        {
            return NotFound(new { message = $"Task with ID '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a task by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _taskService.DeleteTaskAsync(id, ct);
        if (!deleted)
        {
            return NotFound(new { message = $"Task with ID '{id}' was not found." });
        }

        return NoContent();
    }
}