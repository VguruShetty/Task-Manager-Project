using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskItemResponseDto>> GetTasksAsync(TaskQueryParameters parameters, CancellationToken ct = default);
    Task<TaskItemResponseDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskItemResponseDto> CreateTaskAsync(CreateTaskItemDto dto, CancellationToken ct = default);
    Task<bool> UpdateTaskAsync(Guid id, UpdateTaskItemDto dto, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(Guid id, TaskItemStatus newStatus, CancellationToken ct = default);
    Task<bool> DeleteTaskAsync(Guid id, CancellationToken ct = default);
}