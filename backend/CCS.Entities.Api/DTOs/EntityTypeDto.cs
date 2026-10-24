namespace CCS.Entities.Api.DTOs;

/// <summary>Tipo de entidad del catálogo (persona natural, empresa, institución, ONG).</summary>
public class EntityTypeDto
{
    public byte Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Subtipo: 'N' = persona natural, 'L' = persona jurídica.</summary>
    public string Category { get; set; } = string.Empty;
}
