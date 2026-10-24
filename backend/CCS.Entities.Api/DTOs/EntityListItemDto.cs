namespace CCS.Entities.Api.DTOs;

/// <summary>Fila del listado de entidades (proyección de <c>sp_Entity_List</c>).</summary>
public class EntityListItemDto : EntityBaseDto
{
    /// <summary>Nombre para mostrar: "Apellidos, Nombres" o la razón social.</summary>
    public string DisplayName { get; set; } = string.Empty;

    public int PhoneCount { get; set; }
}
