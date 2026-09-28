using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;
using TaskManager.Application.Mappings;

namespace TaskManager.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;

        public TaskService(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TaskItemResponseDto>> GetTasksAsync(Guid? projectId, TaskItemStatus? status, CancellationToken ct = default)
        {
            var entities = await _repository.GetAllAsync(projectId, status, ct);
            return entities.Select(t => t.ToDto());
        }

        public async Task<TaskItemResponseDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            return entity?.ToDto();
        }

        public async Task<TaskItemResponseDto> CreateTaskAsync(CreateTaskItemDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Task title is required.");

            // Manual mapping from DTO to Entity
            var taskEntity = dto.ToEntity();

            var createdEntity = await _repository.AddAsync(taskEntity, dto.TagIds, ct);

            // Fetch back full entity with includes for a complete response
            var reloaded = await _repository.GetByIdAsync(createdEntity.Id, ct);
            return (reloaded ?? createdEntity).ToDto();
        }

        public async Task<bool> UpdateTaskAsync(Guid id, UpdateTaskItemDto dto, CancellationToken ct = default)
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing is null) return false;

            // Manual property mapping on update
            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.Status = dto.Status;
            existing.Priority = dto.Priority;
            existing.DueDateUtc = dto.DueDateUtc;
            existing.AssignedToUserId = dto.AssignedToUserId;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await _repository.UpdateAsync(existing, ct);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, TaskItemStatus newStatus, CancellationToken ct = default)
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing is null) return false;

            existing.Status = newStatus;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await _repository.UpdateAsync(existing, ct);
            return true;
        }

        public async Task<bool> DeleteTaskAsync(Guid id, CancellationToken ct = default)
        {
            return await _repository.DeleteAsync(id, ct);
        }
    }
}
