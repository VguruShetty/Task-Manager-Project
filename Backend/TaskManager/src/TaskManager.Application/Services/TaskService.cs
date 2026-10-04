using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.UserId
        ?? throw new UnauthorizedAccessException("User is not authenticated.");

    public async Task<PagedResult<TaskItemResponseDto>> GetTasksAsync(
        TaskQueryParameters parameters,
        CancellationToken ct = default)
    {
        var pagedEntities = await _taskRepository.GetPagedAsync(parameters, CurrentUserId, ct);

        var mappedDtos = pagedEntities.Items.Select(t => t.ToDto()).ToList();

        return new PagedResult<TaskItemResponseDto>(
            mappedDtos,
            pagedEntities.TotalCount,
            pagedEntities.PageNumber,
            pagedEntities.PageSize
        );
    }

    public async Task<TaskItemResponseDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct);
        if (task is null) return null;

        EnsureAccess(task);

        return task.ToDto();
    }

    public async Task<TaskItemResponseDto> CreateTaskAsync(CreateTaskItemDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Task title is required.");

        var parentProject = await _projectRepository.GetByIdAsync(dto.ProjectId, ct)
            ?? throw new KeyNotFoundException($"Project '{dto.ProjectId}' does not exist.");

        if (parentProject.OwnerId != CurrentUserId)
        {
            throw new UnauthorizedAccessException("You cannot add tasks to a project you do not own.");
        }

        var taskEntity = dto.ToEntity();
        var created = await _taskRepository.AddAsync(taskEntity, dto.TagIds, ct);

        var reloaded = await _taskRepository.GetByIdAsync(created.Id, ct);
        return (reloaded ?? created).ToDto();
    }

    public async Task<bool> UpdateTaskAsync(Guid id, UpdateTaskItemDto dto, CancellationToken ct = default)
    {
        var existing = await _taskRepository.GetByIdAsync(id, ct);
        if (existing is null) return false;

        EnsureAccess(existing);

        existing.Title = dto.Title.Trim();
        existing.Description = dto.Description?.Trim();
        existing.Status = dto.Status;
        existing.Priority = dto.Priority;
        existing.DueDateUtc = dto.DueDateUtc;
        existing.AssignedToUserId = dto.AssignedToUserId;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(existing, ct);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, TaskItemStatus newStatus, CancellationToken ct = default)
    {
        var existing = await _taskRepository.GetByIdAsync(id, ct);
        if (existing is null) return false;

        EnsureAccess(existing);

        existing.Status = newStatus;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(existing, ct);
        return true;
    }

    public async Task<bool> DeleteTaskAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _taskRepository.GetByIdAsync(id, ct);
        if (existing is null) return false;

        if (existing.Project?.OwnerId != CurrentUserId)
        {
            throw new UnauthorizedAccessException("Only the project owner can delete this task.");
        }

        return await _taskRepository.DeleteAsync(id, ct);
    }

    private void EnsureAccess(TaskItem task)
    {
        var userId = CurrentUserId;
        bool isOwner = task.Project?.OwnerId == userId;
        bool isAssignee = task.AssignedToUserId == userId;

        if (!isOwner && !isAssignee)
        {
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }
    }
}