using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks
{
    public class CreateTaskItemDto
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDateUtc { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public List<Guid>? TagIds { get; set; }
    }
}
