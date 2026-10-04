using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Common.Models;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks
{
    public class TaskQueryParameters : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public TaskItemStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime? DueBeforeUtc { get; set; }
    }
}
