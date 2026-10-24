using CCS.Entities.Api.Auth;
using CCS.Entities.Api.Common;
using CCS.Entities.Api.DTOs;
using CCS.Entities.Api.Models;
using CCS.Entities.Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace CCS.Entities.Api.Services;

public class AuthService(
    UserRepository users,
    IPasswordHasher<AppUser> passwordHasher,
    JwtTokenService tokenService)
{
    private const string InvalidCredentials = "Usuario o contraseña incorrectos.";

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await users.GetByUsernameAsync(request.Username);
        if (user is null)
        {
            return Result.Failure<LoginResponseDto>(Error.Unauthorized("auth.invalid_credentials", InvalidCredentials));
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Result.Failure<LoginResponseDto>(Error.Unauthorized("auth.invalid_credentials", InvalidCredentials));
        }

        var (token, expiresAt) = tokenService.GenerateToken(user.Id, user.Username, user.Role);
        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Role = user.Role
        };
    }
}
