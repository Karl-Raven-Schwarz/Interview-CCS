using CCS.Entities.Api.DTOs;
using CCS.Entities.Api.Repositories;

namespace CCS.Entities.Api.Services;

public class CatalogService(CatalogRepository repository)
{
    public async Task<IReadOnlyList<EntityTypeDto>> GetEntityTypesAsync()
    {
        var types = await repository.ListEntityTypesAsync();
        return types.Select(t => new EntityTypeDto
        {
            Id = t.Id,
            Code = t.Code,
            Name = t.Name,
            Category = t.Category
        }).ToList();
    }

    public async Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(string? category)
    {
        var types = await repository.ListDocumentTypesAsync(category?.Trim().ToUpperInvariant());
        return types.Select(t => new DocumentTypeDto
        {
            Id = t.Id,
            Code = t.Code,
            Name = t.Name,
            AllowedFor = t.AllowedFor
        }).ToList();
    }
}
