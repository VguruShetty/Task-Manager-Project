using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default);
        Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<Tag> AddAsync(Tag tag, CancellationToken ct = default);
        Task UpdateAsync(Tag tag, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
