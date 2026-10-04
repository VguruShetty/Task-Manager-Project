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
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _context;

        public TagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Tags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(ct);
        }

        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), ct);
        }

        public async Task<Tag> AddAsync(Tag tag, CancellationToken ct = default)
        {
            await _context.Tags.AddAsync(tag, ct);
            await _context.SaveChangesAsync(ct);
            return tag;
        }

        public async Task UpdateAsync(Tag tag, CancellationToken ct = default)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var tag = await _context.Tags.FindAsync(new object[] { id }, ct);
            if (tag is null) return false;

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
