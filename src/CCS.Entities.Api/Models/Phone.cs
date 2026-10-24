namespace CCS.Entities.Api.Models;

/// <summary>
/// Teléfono de contacto (tabla Phone). Relación 1:N con <see cref="Entity"/>,
/// de modo que aplica a cualquier tipo de entidad.
/// </summary>
public class Phone
{
    public int Id { get; set; }

    public int EntityId { get; set; }

    public string Number { get; set; } = string.Empty;

    /// <summary>Etiqueta libre: Móvil, Casa, Oficina, Central, etc.</summary>
    public string? Label { get; set; }
}
