using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#808080";

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
