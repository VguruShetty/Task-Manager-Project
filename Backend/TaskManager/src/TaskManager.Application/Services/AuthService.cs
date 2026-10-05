using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email, ct))
                throw new InvalidOperationException($"User with email '{dto.Email}' already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                CreatedAtUtc = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);
            return await GenerateAuthResponseWithTokensAsync(user, ct);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
            if (user is null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return await GenerateAuthResponseWithTokensAsync(user, ct);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default)
        {
            var existingRefreshToken = await _refreshTokenRepository.GetByTokenWithUserAsync(dto.RefreshToken, ct);

            if (existingRefreshToken is null || !existingRefreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Refresh token is invalid or has expired.");
            }

            // Token rotation: Revoke the old token upon successful use
            existingRefreshToken.RevokedAtUtc = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(existingRefreshToken, ct);

            var user = existingRefreshToken.User;
            return await GenerateAuthResponseWithTokensAsync(user, ct);
        }

        public async Task<bool> RevokeTokenAsync(RevokeTokenRequestDto dto, CancellationToken ct = default)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, ct);

            if (existingToken is null || !existingToken.IsActive)
                return false;

            existingToken.RevokedAtUtc = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(existingToken, ct);
            return true;
        }

        private async Task<AuthResponseDto> GenerateAuthResponseWithTokensAsync(User user, CancellationToken ct)
        {
            var (jwtToken, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

            // Native .NET cryptographically secure token generation
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var tokenString = Convert.ToBase64String(randomBytes);

            var newRefreshToken = new RefreshToken
            {
                Token = tokenString,
                UserId = user.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                CreatedAtUtc = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken, ct);

            return new AuthResponseDto
            {
                Token = jwtToken,
                RefreshToken = newRefreshToken.Token,
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
