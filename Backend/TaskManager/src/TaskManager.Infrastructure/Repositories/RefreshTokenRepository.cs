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
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken ct = default)
        {
            return await _context.Set<RefreshToken>()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);
        }

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            await _context.Set<RefreshToken>().AddAsync(refreshToken, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            _context.Set<RefreshToken>().Update(refreshToken);
            await _context.SaveChangesAsync(ct);
        }
    }
}
