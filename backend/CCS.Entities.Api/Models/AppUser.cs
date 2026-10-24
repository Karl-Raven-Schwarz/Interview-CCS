namespace CCS.Entities.Api.Models;

public class AppUser
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    /// <summary>Hash de la contraseña (formato ASP.NET Core Identity, con salt embebido).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
