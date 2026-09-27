using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.DTOs.Projects
{
    public class ProjectResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public UserSummaryDto? Owner { get; set; }
        public List<TaskItemResponseDto> Tasks { get; set; } = new();
    }
}
