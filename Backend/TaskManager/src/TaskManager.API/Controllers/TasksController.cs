using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.API.Controllers
{
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
        /// Get all tasks with optional filters by projectId and status.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TaskItemResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TaskItemResponseDto>>> GetAll(
            [FromQuery] Guid? projectId,
            [FromQuery] TaskItemStatus? status,
            CancellationToken ct)
        {
            var tasks = await _taskService.GetTasksAsync(projectId, status, ct);
            return Ok(tasks);
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
        public async Task<ActionResult<TaskItemResponseDto>> Create(
            [FromBody] CreateTaskItemDto dto,
            CancellationToken ct)
        {
            try
            {
                var created = await _taskService.CreateTaskAsync(dto, ct);
                return CreatedAtRoute(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update all fields of an existing task.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
}
