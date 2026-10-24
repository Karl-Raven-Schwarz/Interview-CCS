namespace CCS.Entities.Api.DTOs;

/// <summary>Tipo de documento del catálogo (DNI, CE, Pasaporte, RUC, Partida registral).</summary>
public class DocumentTypeDto
{
    public byte Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>'N' solo natural, 'L' solo jurídica, 'B' ambos.</summary>
    public string AllowedFor { get; set; } = "B";
}
