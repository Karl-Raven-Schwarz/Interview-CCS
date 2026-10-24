using System.ComponentModel.DataAnnotations;
using CCS.Entities.Api.DTOs;
using CCS.Entities.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCS.Entities.Api.Controllers;

[ApiController]
[Route("api/catalogs")]
[Authorize]
public class CatalogsController(CatalogService catalogs) : ControllerBase
{
    [HttpGet("entity-types")]
    public async Task<ActionResult<IReadOnlyList<EntityTypeDto>>> GetEntityTypes() =>
        Ok(await catalogs.GetEntityTypesAsync());

    /// <summary>Filtra por categoría de entidad: 'N' natural, 'L' jurídica.</summary>
    [HttpGet("document-types")]
    public async Task<ActionResult<IReadOnlyList<DocumentTypeDto>>> GetDocumentTypes(
        [FromQuery, RegularExpression("^[NL]$", ErrorMessage = "La categoría debe ser 'N' (natural) o 'L' (jurídica).")]
        string? category) =>
        Ok(await catalogs.GetDocumentTypesAsync(category));
}
