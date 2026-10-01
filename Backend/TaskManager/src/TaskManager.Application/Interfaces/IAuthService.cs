using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Auth;

namespace TaskManager.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    }
}
