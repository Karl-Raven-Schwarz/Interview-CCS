namespace CCS.Entities.Api.DTOs;

/// <summary>Token JWT emitido tras un login exitoso.</summary>
public class LoginResponseDto
{
    /// <summary>Token JWT para enviar en la cabecera Authorization.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Siempre "Bearer".</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>Fecha y hora (UTC) de expiración del token.</summary>
    public DateTime ExpiresAt { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
