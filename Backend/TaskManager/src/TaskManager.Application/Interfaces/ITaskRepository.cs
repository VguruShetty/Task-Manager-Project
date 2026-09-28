using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync(Guid? projectId, TaskItemStatus? status, CancellationToken ct = default);
        Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<TaskItem> AddAsync(TaskItem task, List<Guid>? tagIds, CancellationToken ct = default);
        Task UpdateAsync(TaskItem task, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
