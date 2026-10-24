using System.ComponentModel.DataAnnotations;

namespace CCS.Entities.Api.DTOs;

/// <summary>Filtros del listado de entidades (query string de GET /api/entities).</summary>
public class EntityQueryDto
{
    /// <summary>Busca por documento, nombres o razón social. Vacío = sin filtro.</summary>
    [StringLength(100, ErrorMessage = "La búsqueda no puede exceder 100 caracteres.")]
    public string? Search { get; set; }

    /// <summary>Filtra por tipo de entidad.</summary>
    [Range(1, 255, ErrorMessage = "El tipo de entidad no es válido.")]
    public byte? EntityTypeId { get; set; }
}
