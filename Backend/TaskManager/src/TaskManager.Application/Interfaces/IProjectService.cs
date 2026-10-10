using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Projects;

namespace TaskManager.Application.Interfaces;

public interface IProjectService
{
    Task<PagedResult<ProjectResponseDto>> GetProjectsAsync(
        ProjectQueryParameters parameters,
        CancellationToken ct = default);

    Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken ct = default);
    Task<bool> UpdateProjectAsync(Guid id, UpdateProjectDto dto, CancellationToken ct = default);
    Task<bool> DeleteProjectAsync(Guid id, CancellationToken ct = default);
}