using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync(Guid? projectId, TaskItemStatus? status, CancellationToken ct = default)
        {
            var query = _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.Tags)
                .AsNoTracking()
                .AsQueryable();

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            return await query.ToListAsync(ct);
        }

        public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<TaskItem> AddAsync(TaskItem task, List<Guid>? tagIds, CancellationToken ct = default)
        {
            if (tagIds is { Count: > 0 })
            {
                var tags = await _context.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync(ct);
                task.Tags = tags;
            }

            await _context.Tasks.AddAsync(task, ct);
            await _context.SaveChangesAsync(ct);
            return task;
        }

        public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var task = await _context.Tasks.FindAsync(new object[] { id }, ct);
            if (task is null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
