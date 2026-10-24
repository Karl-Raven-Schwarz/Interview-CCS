namespace CCS.Entities.Api.Models;

/// <summary>
/// Catálogo de tipos de entidad (tabla EntityType): persona natural, empresa,
/// institución, ONG, etc. Agregar un tipo nuevo es insertar una fila.
/// </summary>
public class EntityType
{
    public byte Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Subtipo al que pertenece: 'N' = NaturalPerson, 'L' = LegalPerson.</summary>
    public string Category { get; set; } = string.Empty;
}
