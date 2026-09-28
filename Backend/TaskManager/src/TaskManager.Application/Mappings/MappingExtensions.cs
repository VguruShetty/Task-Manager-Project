using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.DTOs.Tags;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Mappings
{
    public static class MappingExtensions // Must be public static
    {
        // Must be public static and have 'this TaskItem task'
        public static TaskItemResponseDto ToDto(this TaskItem task)
        {
            return new TaskItemResponseDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDateUtc = task.DueDateUtc,
                CreatedAtUtc = task.CreatedAtUtc,
                UpdatedAtUtc = task.UpdatedAtUtc,
                AssignedToUser = task.AssignedToUser != null
                    ? new UserSummaryDto
                    {
                        Id = task.AssignedToUser.Id,
                        FullName = task.AssignedToUser.FullName,
                        Email = task.AssignedToUser.Email
                    }
                    : null,
                Tags = task.Tags.Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ColorHex = t.ColorHex
                }).ToList()
            };
        }

        public static TaskItem ToEntity(this CreateTaskItemDto dto)
        {
            return new TaskItem
            {
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDateUtc = dto.DueDateUtc,
                AssignedToUserId = dto.AssignedToUserId
            };
        }

        public static ProjectResponseDto ToDto(this Project project)
        {
            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAtUtc = project.CreatedAtUtc,
                Owner = project.Owner != null
                    ? new UserSummaryDto
                    {
                        Id = project.Owner.Id,
                        FullName = project.Owner.FullName,
                        Email = project.Owner.Email
                    }
                    : null,
                Tasks = project.Tasks.Select(t => t.ToDto()).ToList()
            };
        }

        public static Project ToEntity(this CreateProjectDto dto)
        {
            return new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                OwnerId = dto.OwnerId
            };
        }
    }
}
