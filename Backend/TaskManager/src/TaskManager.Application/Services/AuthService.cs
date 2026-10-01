using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
        {
            // 1. Ensure email uniqueness
            var emailExists = await _userRepository.ExistsByEmailAsync(dto.Email, ct);
            if (emailExists)
            {
                throw new InvalidOperationException($"User with email '{dto.Email}' already exists.");
            }

            // 2. Hash password and build entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                CreatedAtUtc = DateTime.UtcNow
            };

            // 3. Save to database
            await _userRepository.AddAsync(user, ct);

            // 4. Generate JWT
            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresAtUtc = expiresAtUtc,
                User = new UserSummaryDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
        {
            // 1. Fetch user by email
            var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // 2. Verify hashed password
            var isPasswordValid = _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // 3. Issue token
            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresAtUtc = expiresAtUtc,
                User = new UserSummaryDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email
                }
            };
        }
    }
}
