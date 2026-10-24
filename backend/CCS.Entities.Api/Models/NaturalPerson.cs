namespace CCS.Entities.Api.Models;

/// <summary>
/// Subtipo de <see cref="Entity"/> para personas naturales (tabla NaturalPerson).
/// Conserva literalmente los campos exigidos en el requerimiento:
/// nombres, apellido paterno, apellido materno, edad y sexo.
/// </summary>
public class NaturalPerson
{
    /// <summary>Clave primaria y foránea a Entity.Id (herencia por extensión).</summary>
    public int EntityId { get; set; }

    public string Names { get; set; } = string.Empty;

    public string PaternalSurname { get; set; } = string.Empty;

    public string MaternalSurname { get; set; } = string.Empty;

    /// <summary>Entre 1 y 130.</summary>
    public byte Age { get; set; }

    /// <summary>'M' o 'F'.</summary>
    public string Sex { get; set; } = string.Empty;
}
