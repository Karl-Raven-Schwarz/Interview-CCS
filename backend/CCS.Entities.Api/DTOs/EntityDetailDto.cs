namespace CCS.Entities.Api.DTOs;

/// <summary>Detalle de una entidad con su lista de teléfonos (<c>sp_Entity_GetById</c>).</summary>
public class EntityDetailDto : EntityBaseDto
{
    /// <summary>Teléfonos de contacto (1:N).</summary>
    public List<PhoneDto> Phones { get; set; } = [];
}
