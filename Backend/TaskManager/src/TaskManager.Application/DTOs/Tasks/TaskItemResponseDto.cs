using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Tags;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks
{
    public class TaskItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskItemStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDateUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public UserSummaryDto? AssignedToUser { get; set; }
        public List<TagDto> Tags { get; set; } = new();
    }
}
