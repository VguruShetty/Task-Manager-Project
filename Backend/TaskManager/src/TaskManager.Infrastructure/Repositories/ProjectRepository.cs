using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllAsync(Guid? ownerId, CancellationToken ct = default)
        {
            var query = _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Tags)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedToUser)
                .AsNoTracking()
                .AsQueryable();

            if (ownerId.HasValue)
                query = query.Where(p => p.OwnerId == ownerId.Value);

            return await query.ToListAsync(ct);
        }

        public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Tags)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedToUser)
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
}
