using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Tags;

namespace TaskManager.Application.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllAsync(CancellationToken ct = default);
        Task<TagDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<TagDto> CreateAsync(CreateTagDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
