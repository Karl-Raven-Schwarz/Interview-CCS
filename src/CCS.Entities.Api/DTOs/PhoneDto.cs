using System.ComponentModel.DataAnnotations;

namespace CCS.Entities.Api.DTOs;

/// <summary>
/// Teléfono de contacto. Se usa en las respuestas (con <see cref="Id"/>)
/// y en las peticiones de creación/modificación (el Id se ignora).
/// </summary>
public class PhoneDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "El teléfono debe tener entre 6 y 20 caracteres.")]
    [RegularExpression(@"^[0-9+\-\s()]+$", ErrorMessage = "El teléfono solo puede contener dígitos, espacios y los símbolos + - ( ).")]
    public string Number { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "La etiqueta no puede exceder 30 caracteres.")]
    public string? Label { get; set; }
}
