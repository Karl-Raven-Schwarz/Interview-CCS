namespace CCS.Entities.Api.DTOs;

/// <summary>
/// Datos comunes a cualquier vista de una entidad. El supertipo vive en la tabla
/// Entity; lo propio de cada tipo en NaturalPerson o LegalPerson (herencia por extensión).
/// </summary>
public class EntityBaseDto
{
    public int Id { get; set; }

    public string EntityTypeCode { get; set; } = string.Empty;

    public string EntityTypeName { get; set; } = string.Empty;

    /// <summary>'N' = persona natural, 'L' = persona jurídica.</summary>
    public string EntityCategory { get; set; } = string.Empty;

    public byte DocumentTypeId { get; set; }

    public string DocumentTypeCode { get; set; } = string.Empty;

    public string DocumentTypeName { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    // --- Persona natural (null si es jurídica) ---
    public string? Names { get; set; }

    public string? PaternalSurname { get; set; }

    public string? MaternalSurname { get; set; }

    public byte? Age { get; set; }

    public string? Sex { get; set; }

    // --- Persona jurídica (null si es natural) ---
    public string? LegalName { get; set; }

    public string? TradeName { get; set; }

    public string? ContactName { get; set; }

    // --- Datos comunes ---
    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    /// <summary>UTC (SQL Server devuelve la fecha sin zona).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC (SQL Server devuelve la fecha sin zona).</summary>
    public DateTime? UpdatedAt { get; set; }
}
