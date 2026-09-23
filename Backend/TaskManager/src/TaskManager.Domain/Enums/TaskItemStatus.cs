using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Domain.Enums
{
    public enum TaskItemStatus
    {
        Todo=0,
        InProgress=1,
        InReview=2,
        Completed=3,
    }
}
