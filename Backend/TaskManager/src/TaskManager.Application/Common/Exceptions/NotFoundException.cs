using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with key '{key}' was not found.")
        {
        }
    }
}
