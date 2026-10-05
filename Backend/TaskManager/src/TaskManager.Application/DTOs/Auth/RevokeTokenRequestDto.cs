using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.DTOs.Auth
{
    public class RevokeTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
