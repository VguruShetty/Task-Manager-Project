using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs.Projects; // Added this import
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    public interface IProjectRepository
    {
        Task<PagedResult<Project>> GetPagedAsync(
            ProjectQueryParameters parameters,
            Guid ownerId,
            CancellationToken ct = default);

        Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Project> AddAsync(Project project, CancellationToken ct = default);
        Task UpdateAsync(Project project, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}