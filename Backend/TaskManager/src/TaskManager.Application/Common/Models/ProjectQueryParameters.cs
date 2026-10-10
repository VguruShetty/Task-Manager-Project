using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.Common.Models
{
    public class ProjectQueryParameters : PaginationParams
    {
        /// <summary>
        /// Filter projects created on or after this UTC date.
        /// </summary>
        public DateTime? CreatedAfterUtc { get; set; }

        /// <summary>
        /// Filter projects created on or before this UTC date.
        /// </summary>
        public DateTime? CreatedBeforeUtc { get; set; }

        /// <summary>
        /// Optional filter to only show projects that have at least one task, or empty projects.
        /// </summary>
        public bool? HasTasks { get; set; }
    }
}
