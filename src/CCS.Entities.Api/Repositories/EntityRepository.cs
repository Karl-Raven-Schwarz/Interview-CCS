using System.Data;
using System.Text.Json;
using CCS.Entities.Api.Data;
using CCS.Entities.Api.DTOs;
using CCS.Entities.Api.Models;
using Dapper;

namespace CCS.Entities.Api.Repositories;

public class EntityRepository(DapperContext context)
{
    public async Task<IReadOnlyList<EntityListItemDto>> ListAsync(string? search, byte? entityTypeId)
    {
        using var connection = context.CreateConnection();
        var rows = await connection.QueryAsync<EntityListItemDto>(
            "sp_Entity_List",
            new { Search = search, EntityTypeId = entityTypeId },
            commandType: CommandType.StoredProcedure);
        return rows.AsList();
    }

    /// <summary>Detalle con teléfonos (sp_Entity_GetById devuelve dos result sets).</summary>
    public async Task<EntityDetailDto?> GetByIdAsync(int id)
    {
        using var connection = context.CreateConnection();
        using var grid = await connection.QueryMultipleAsync(
            "sp_Entity_GetById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        var detail = await grid.ReadSingleOrDefaultAsync<EntityDetailDto>();
        if (detail is null)
        {
            return null;
        }

        detail.Phones = (await grid.ReadAsync<PhoneDto>()).AsList();
        return detail;
    }

    public async Task<int> InsertAsync(Entity entity, NaturalPerson? natural, LegalPerson? legal, IReadOnlyList<Phone> phones)
    {
        using var connection = context.CreateConnection();
        var parameters = BuildParameters(entity, natural, legal, phones);
        parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_Entity_Insert", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@NewId");
    }

    public async Task UpdateAsync(Entity entity, NaturalPerson? natural, LegalPerson? legal, IReadOnlyList<Phone> phones)
    {
        using var connection = context.CreateConnection();
        var parameters = BuildParameters(entity, natural, legal, phones);
        parameters.Add("@Id", entity.Id);
        parameters.Add("@IsActive", entity.IsActive);

        await connection.ExecuteAsync("sp_Entity_Update", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = context.CreateConnection();
        await connection.ExecuteAsync("sp_Entity_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
    }

    private static DynamicParameters BuildParameters(Entity entity, NaturalPerson? natural, LegalPerson? legal, IReadOnlyList<Phone> phones)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EntityTypeId", entity.EntityTypeId);
        parameters.Add("@DocumentTypeId", entity.DocumentTypeId);
        parameters.Add("@DocumentNumber", entity.DocumentNumber);
        parameters.Add("@Email", entity.Email);
        parameters.Add("@Address", entity.Address);
        parameters.Add("@Names", natural?.Names);
        parameters.Add("@PaternalSurname", natural?.PaternalSurname);
        parameters.Add("@MaternalSurname", natural?.MaternalSurname);
        parameters.Add("@Age", natural?.Age);
        parameters.Add("@Sex", natural?.Sex);
        parameters.Add("@LegalName", legal?.LegalName);
        parameters.Add("@TradeName", legal?.TradeName);
        parameters.Add("@ContactName", legal?.ContactName);
        parameters.Add("@PhonesJson", ToPhonesJson(phones));
        return parameters;
    }

    private static string? ToPhonesJson(IReadOnlyList<Phone> phones) =>
        phones.Count == 0
            ? null
            : JsonSerializer.Serialize(phones.Select(p => new { number = p.Number, label = p.Label }));
}
