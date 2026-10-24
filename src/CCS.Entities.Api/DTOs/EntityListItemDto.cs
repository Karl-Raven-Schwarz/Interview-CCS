namespace CCS.Entities.Api.DTOs;

/// <summary>
/// Fila del listado de entidades (resultado de <c>sp_Entity_List</c>).
/// Refleja la proyección plana del procedimiento: datos comunes del supertipo,
/// los del subtipo según el caso y el nombre calculado para mostrar.
/// </summary>
public class EntityListItemDto
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

    /// <summary>Nombre para mostrar: "Apellidos, Nombres" o la razón social.</summary>
    public string DisplayName { get; set; } = string.Empty;

    public int PhoneCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
