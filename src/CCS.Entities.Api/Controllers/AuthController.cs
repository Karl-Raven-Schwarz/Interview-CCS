using CCS.Entities.Api.Common;
using CCS.Entities.Api.DTOs;
using CCS.Entities.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCS.Entities.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var result = await authService.LoginAsync(request);
        if (result.IsFailure)
        {
            Response.Headers["WWW-Authenticate"] = "Bearer";
            return result.ToProblem();
        }

        return result.Value;
    }
}
