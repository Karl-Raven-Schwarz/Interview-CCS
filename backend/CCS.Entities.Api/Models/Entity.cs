namespace CCS.Entities.Api.Models;

/// <summary>
/// Entidad del negocio (tabla Entity): supertipo de toda entidad
/// (persona, empresa, institución, etc.). Concentra los datos comunes;
/// lo propio de cada tipo vive en <see cref="NaturalPerson"/> o <see cref="LegalPerson"/>.
/// </summary>
public class Entity
{
    public int Id { get; set; }

    public byte EntityTypeId { get; set; }

    public byte DocumentTypeId { get; set; }

    public string DocumentNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
