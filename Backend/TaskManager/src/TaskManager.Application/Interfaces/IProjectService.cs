using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Projects;

namespace TaskManager.Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponseDto>> GetAllAsync(Guid? ownerId, CancellationToken ct = default);
        Task<ProjectResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
