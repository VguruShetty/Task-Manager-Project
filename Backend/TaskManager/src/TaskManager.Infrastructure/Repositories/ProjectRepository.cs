using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Extensions;

namespace TaskManager.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Project>> GetPagedAsync(
        ProjectQueryParameters parameters,
        Guid ownerId,
        CancellationToken ct = default)
    {
        var query = _context.Projects
            .Include(p => p.Tasks)
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId);

        // --- Free-text Search ---
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) ||
                                     (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        // --- Optional Date Filters from ProjectQueryParameters ---
        if (parameters.CreatedAfterUtc.HasValue)
        {
            query = query.Where(p => p.CreatedAtUtc >= parameters.CreatedAfterUtc.Value);
        }

        if (parameters.CreatedBeforeUtc.HasValue)
        {
            query = query.Where(p => p.CreatedAtUtc <= parameters.CreatedBeforeUtc.Value);
        }

        if (parameters.HasTasks.HasValue)
        {
            query = parameters.HasTasks.Value
                ? query.Where(p => p.Tasks.Any())
                : query.Where(p => !p.Tasks.Any());
        }

        // --- Sorting ---
        // Notice: Use p.Tasks.Count() with parentheses so EF Core translates it to COUNT(*) in SQL
        query = parameters.SortBy?.ToLower() switch
        {
            "name" => parameters.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "taskcount" => parameters.IsDescending ? query.OrderByDescending(p => p.Tasks.Count()) : query.OrderBy(p => p.Tasks.Count()),
            "updatedat" => parameters.IsDescending ? query.OrderByDescending(p => p.UpdatedAtUtc) : query.OrderBy(p => p.UpdatedAtUtc),
            _ => parameters.IsDescending ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc)
        };

        return await query.ToPagedResultAsync(parameters.PageNumber, parameters.PageSize, ct);
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project> AddAsync(Project project, CancellationToken ct = default)
    {
        await _context.Projects.AddAsync(project, ct);
        await _context.SaveChangesAsync(ct);
        return project;
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, ct);
        if (project is null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}