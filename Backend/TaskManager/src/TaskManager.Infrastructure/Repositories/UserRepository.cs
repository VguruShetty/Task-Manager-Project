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
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower(), ct);
        }

        public async Task<User> AddAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
            return user;
        }
    }
}
