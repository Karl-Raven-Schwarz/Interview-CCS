namespace CCS.Entities.Api.Models;

/// <summary>
/// Subtipo de <see cref="Entity"/> para personas jurídicas (tabla LegalPerson).
/// El campo exigido por el requerimiento es la razón social
/// (<see cref="LegalName"/>); el resto es información conveniente.
/// </summary>
public class LegalPerson
{
    /// <summary>Clave primaria y foránea a Entity.Id (herencia por extensión).</summary>
    public int EntityId { get; set; }

    /// <summary>Razón social (obligatoria).</summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>Nombre comercial.</summary>
    public string? TradeName { get; set; }

    /// <summary>Persona de contacto.</summary>
    public string? ContactName { get; set; }
}
