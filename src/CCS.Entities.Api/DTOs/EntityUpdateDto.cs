using System.ComponentModel.DataAnnotations;

namespace CCS.Entities.Api.DTOs;

/// <summary>
/// Modificación de una entidad. Misma forma que el alta, más el estado.
/// </summary>
public class EntityUpdateDto : EntityCreateDto
{
    // bool? para que su ausencia sea un 400: con bool no-nullable el default
    // silencioso era true y reactivaba la entidad.
    [Required(ErrorMessage = "El estado de la entidad es obligatorio.")]
    public bool? IsActive { get; set; }
}
