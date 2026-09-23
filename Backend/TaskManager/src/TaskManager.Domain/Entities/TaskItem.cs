using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDateUtc { get; set; }

        // Audit fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // Relationships
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
