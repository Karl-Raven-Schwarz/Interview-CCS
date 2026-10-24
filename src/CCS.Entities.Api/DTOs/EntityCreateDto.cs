using System.ComponentModel.DataAnnotations;

namespace CCS.Entities.Api.DTOs;

/// <summary>
/// Alta de una entidad. Reúne los datos comunes del supertipo y los de ambos
/// subtipos; el servicio valida cuáles son obligatorios según la categoría del
/// tipo de entidad elegido ('N' natural / 'L' jurídica), tal como hace
/// <c>sp_Entity_Insert</c> en la base de datos.
/// </summary>
public class EntityCreateDto
{
    [Range(1, 255, ErrorMessage = "El tipo de entidad no es válido.")]
    public byte EntityTypeId { get; set; }

    [Range(1, 255, ErrorMessage = "El tipo de documento no es válido.")]
    public byte DocumentTypeId { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "El número de documento debe tener entre 6 y 20 caracteres.")]
    public string DocumentNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(120, ErrorMessage = "El correo no puede exceder 120 caracteres.")]
    public string? Email { get; set; }

    [StringLength(150, ErrorMessage = "La dirección no puede exceder 150 caracteres.")]
    public string? Address { get; set; }

    // --- Persona natural (obligatorios si la categoría es 'N') ---

    [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres.")]
    public string? Names { get; set; }

    [StringLength(60, ErrorMessage = "El apellido paterno no puede exceder 60 caracteres.")]
    public string? PaternalSurname { get; set; }

    [StringLength(60, ErrorMessage = "El apellido materno no puede exceder 60 caracteres.")]
    public string? MaternalSurname { get; set; }

    [Range(1, 130, ErrorMessage = "La edad debe estar entre 1 y 130.")]
    public byte? Age { get; set; }

    [RegularExpression("^[MF]$", ErrorMessage = "El sexo debe ser 'M' o 'F'.")]
    public string? Sex { get; set; }

    // --- Persona jurídica (obligatorio si la categoría es 'L') ---

    [StringLength(150, ErrorMessage = "La razón social no puede exceder 150 caracteres.")]
    public string? LegalName { get; set; }

    [StringLength(150, ErrorMessage = "El nombre comercial no puede exceder 150 caracteres.")]
    public string? TradeName { get; set; }

    [StringLength(120, ErrorMessage = "El contacto no puede exceder 120 caracteres.")]
    public string? ContactName { get; set; }

    /// <summary>Lista de teléfonos de contacto (puede venir vacía).</summary>
    public List<PhoneDto> Phones { get; set; } = [];
}
