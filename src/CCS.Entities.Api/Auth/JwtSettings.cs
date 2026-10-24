namespace CCS.Entities.Api.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    /// <summary>Duración del token en minutos. Sin default en el código: lo fija la configuración.</summary>
    public int ExpirationMinutes { get; set; }

    /// <summary>Tolerancia de reloj en minutos. Sin default en el código: lo fija la configuración.</summary>
    public int ClockSkewMinutes { get; set; }
}
