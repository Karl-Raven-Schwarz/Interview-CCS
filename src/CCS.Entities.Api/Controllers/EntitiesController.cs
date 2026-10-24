using CCS.Entities.Api.Common;
using CCS.Entities.Api.DTOs;
using CCS.Entities.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCS.Entities.Api.Controllers;

[ApiController]
[Route("api/entities")]
[Authorize]
public class EntitiesController(EntityService entities) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EntityListItemDto>>> List([FromQuery] EntityQueryDto query) =>
        Ok(await entities.ListAsync(query));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EntityDetailDto>> GetById(int id)
    {
        var result = await entities.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return result.Value;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EntityDetailDto>> Create(EntityCreateDto request)
    {
        var result = await entities.CreateAsync(request);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, EntityUpdateDto request)
    {
        var result = await entities.UpdateAsync(id, request);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await entities.DeleteAsync(id);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return NoContent();
    }
}
