using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
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

        public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync(Guid? ownerId, CancellationToken ct = default)
        {
            // Enforce: Always scope down to the authenticated user's ID
            var projects = await _projectRepository.GetAllAsync(CurrentUserId, ct);
            return projects.Select(p => p.ToDto());
        }

        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var project = await _projectRepository.GetByIdAsync(id, ct);
            if (project is null) return null;

            // Prevent unauthorized inspection of another user's project
            if (project.OwnerId != CurrentUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this project.");
            }

            return project.ToDto();
        }

        public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Project name is required.");

            var entity = new Project
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                OwnerId = CurrentUserId // Derived securely from claims, not user input
            };

            var created = await _projectRepository.AddAsync(entity, ct);
            var fullEntity = await _projectRepository.GetByIdAsync(created.Id, ct);

            return (fullEntity ?? created).ToDto();
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto, CancellationToken ct = default)
        {
            var existing = await _projectRepository.GetByIdAsync(id, ct);
            if (existing is null) return false;

            // Security check
            if (existing.OwnerId != CurrentUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this project.");
            }

            existing.Name = dto.Name.Trim();
            existing.Description = dto.Description?.Trim();

            await _projectRepository.UpdateAsync(existing, ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await _projectRepository.GetByIdAsync(id, ct);
            if (existing is null) return false;

            // Security check
            if (existing.OwnerId != CurrentUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this project.");
            }

            return await _projectRepository.DeleteAsync(id, ct);
        }
    }
}
