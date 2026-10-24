namespace CCS.Entities.Api.Models;

/// <summary>Catálogo de tipos de documento (tabla DocumentType): DNI, RUC, CE, etc.</summary>
public class DocumentType
{
    public byte Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A qué categoría de entidad aplica:
    /// 'N' solo persona natural, 'L' solo jurídica, 'B' ambos.
    /// </summary>
    public string AllowedFor { get; set; } = "B";
}
