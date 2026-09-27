using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks
{
    public class UpdateTaskStatusDto
    {
        public TaskItemStatus Status { get; set; }
    }
}
