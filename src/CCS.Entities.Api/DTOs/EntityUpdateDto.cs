using System.ComponentModel.DataAnnotations;

namespace CCS.Entities.Api.DTOs;

/// <summary>
/// Modificación de una entidad. Misma forma que el alta, más el estado.
/// </summary>
public class EntityUpdateDto : EntityCreateDto
{
    [Required(ErrorMessage = "El estado de la entidad es obligatorio.")]
    public bool IsActive { get; set; } = true;
}
