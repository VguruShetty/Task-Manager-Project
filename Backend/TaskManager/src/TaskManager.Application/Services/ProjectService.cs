using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;

namespace TaskManager.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync(Guid? ownerId, CancellationToken ct = default)
        {
            var projects = await _projectRepository.GetAllAsync(ownerId, ct);
            return projects.Select(p => p.ToDto());
        }

        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var project = await _projectRepository.GetByIdAsync(id, ct);
            return project?.ToDto();
        }

        public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Project name is required.");

            var entity = dto.ToEntity();
            var created = await _projectRepository.AddAsync(entity, ct);
            var fullEntity = await _projectRepository.GetByIdAsync(created.Id, ct);

            return (fullEntity ?? created).ToDto();
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto, CancellationToken ct = default)
        {
            var existing = await _projectRepository.GetByIdAsync(id, ct);
            if (existing is null) return false;

            existing.Name = dto.Name;
            existing.Description = dto.Description;

            await _projectRepository.UpdateAsync(existing, ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _projectRepository.DeleteAsync(id, ct);
        }
    }
}
