using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Extensions;

namespace TaskManager.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        //public async Task<IEnumerable<TaskItem>> GetAllAsync(Guid? projectId, TaskItemStatus? status, CancellationToken ct = default)
        //{
        //    var query = _context.Tasks
        //        .Include(t => t.AssignedToUser)
        //        .Include(t => t.Tags)
        //        .AsNoTracking()
        //        .AsQueryable();

        //    if (projectId.HasValue)
        //        query = query.Where(t => t.ProjectId == projectId.Value);

        //    if (status.HasValue)
        //        query = query.Where(t => t.Status == status.Value);

        //    return await query.ToListAsync(ct);
        //}
        public async Task<PagedResult<TaskItem>> GetPagedAsync(TaskQueryParameters parameters, Guid currentUserId, CancellationToken ct = default)
        {
            var query = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Tags)
                .AsNoTracking()
                // Tenant isolation: current user must own the project or be assigned
                .Where(t => t.Project.OwnerId == currentUserId || t.AssignedToUserId == currentUserId);

            // --- Filtering ---
            if (parameters.ProjectId.HasValue)
                query = query.Where(t => t.ProjectId == parameters.ProjectId.Value);

            if (parameters.Status.HasValue)
                query = query.Where(t => t.Status == parameters.Status.Value);

            if (parameters.Priority.HasValue)
                query = query.Where(t => t.Priority == parameters.Priority.Value);

            if (parameters.AssignedToUserId.HasValue)
                query = query.Where(t => t.AssignedToUserId == parameters.AssignedToUserId.Value);

            if (parameters.DueBeforeUtc.HasValue)
                query = query.Where(t => t.DueDateUtc.HasValue && t.DueDateUtc.Value <= parameters.DueBeforeUtc.Value);

            // --- Free-text Search ---
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.Trim().ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(search) ||
                                         (t.Description != null && t.Description.ToLower().Contains(search)));
            }

            // --- Sorting ---
            query = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.IsDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "priority" => parameters.IsDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                "status" => parameters.IsDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "duedate" => parameters.IsDescending ? query.OrderByDescending(t => t.DueDateUtc) : query.OrderBy(t => t.DueDateUtc),
                _ => parameters.IsDescending ? query.OrderByDescending(t => t.CreatedAtUtc) : query.OrderBy(t => t.CreatedAtUtc)
            };

            return await query.ToPagedResultAsync(parameters.PageNumber, parameters.PageSize, ct);
        }
        public async Task<IEnumerable<TaskItem>> GetAllAsync(Guid? projectId, TaskItemStatus? status, CancellationToken ct = default)
        {
            var query = _context.Tasks
                .Include(t => t.Project) // Required for OwnerId checking
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

        //public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        //{
        //    return await _context.Tasks
        //        .Include(t => t.AssignedToUser)
        //        .Include(t => t.Tags)
        //        .FirstOrDefaultAsync(t => t.Id == id, ct);
        //}
        public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Tasks
                .Include(t => t.Project) // Required for OwnerId checking
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
