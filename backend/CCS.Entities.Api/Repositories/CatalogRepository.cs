using System.Data;
using CCS.Entities.Api.Data;
using CCS.Entities.Api.Models;
using Dapper;

namespace CCS.Entities.Api.Repositories;

public class CatalogRepository(DapperContext context)
{
    public async Task<IReadOnlyList<EntityType>> ListEntityTypesAsync()
    {
        using var connection = context.CreateConnection();
        var rows = await connection.QueryAsync<EntityType>(
            "sp_EntityType_List",
            commandType: CommandType.StoredProcedure);
        return rows.AsList();
    }

    public async Task<IReadOnlyList<DocumentType>> ListDocumentTypesAsync(string? category)
    {
        using var connection = context.CreateConnection();
        var rows = await connection.QueryAsync<DocumentType>(
            "sp_DocumentType_List",
            new { Category = category },
            commandType: CommandType.StoredProcedure);
        return rows.AsList();
    }
}
