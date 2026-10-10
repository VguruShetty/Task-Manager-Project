using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public ProjectService(
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.UserId
        ?? throw new UnauthorizedAccessException("User is not authenticated.");

    public async Task<PagedResult<ProjectResponseDto>> GetProjectsAsync(
        ProjectQueryParameters parameters,
        CancellationToken ct = default)
    {
        var pagedEntities = await _projectRepository.GetPagedAsync(parameters, CurrentUserId, ct);

        var mappedDtos = pagedEntities.Items.Select(p => p.ToDto()).ToList();

        return new PagedResult<ProjectResponseDto>(
            mappedDtos,
            pagedEntities.TotalCount,
            pagedEntities.PageNumber,
            pagedEntities.PageSize
        );
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project is null) return null;

        EnsureOwnership(project);

        return project.ToDto();
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken ct = default)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            OwnerId = CurrentUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        var created = await _projectRepository.AddAsync(project, ct);
        return created.ToDto();
    }

    public async Task<bool> UpdateProjectAsync(Guid id, UpdateProjectDto dto, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project is null) return false;

        EnsureOwnership(project);

        project.Name = dto.Name.Trim();
        project.Description = dto.Description?.Trim();
        project.UpdatedAtUtc = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, ct);
        return true;
    }

    public async Task<bool> DeleteProjectAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project is null) return false;

        EnsureOwnership(project);

        return await _projectRepository.DeleteAsync(id, ct);
    }

    private void EnsureOwnership(Project project)
    {
        if (project.OwnerId != CurrentUserId)
        {
            throw new UnauthorizedAccessException("You do not have access to this project.");
        }
    }
}