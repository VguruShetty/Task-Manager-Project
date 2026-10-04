using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.DTOs.Tags
{
    public class UpdateTagDto
    {
        public string Name { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#808080";
    }
}
