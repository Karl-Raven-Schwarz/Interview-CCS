using CCS.Entities.Api.Common;
using CCS.Entities.Api.DTOs;
using CCS.Entities.Api.Models;
using CCS.Entities.Api.Repositories;
using Microsoft.Data.SqlClient;

namespace CCS.Entities.Api.Services;

public class EntityService(EntityRepository entities, CatalogRepository catalogs)
{
    private const int MaxPhones = 10;

    public async Task<IReadOnlyList<EntityListItemDto>> ListAsync(EntityQueryDto query)
    {
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        return await entities.ListAsync(search, query.EntityTypeId);
    }

    public async Task<Result<EntityDetailDto>> GetByIdAsync(int id)
    {
        var detail = await entities.GetByIdAsync(id);
        if (detail is null)
        {
            return Result.Failure<EntityDetailDto>(Error.NotFound("entity.not_found", "La entidad indicada no existe."));
        }

        return detail;
    }

    public async Task<Result<EntityDetailDto>> CreateAsync(EntityCreateDto request)
    {
        var (error, category) = await ValidateAsync(request);
        if (error is not null)
        {
            return Result.Failure<EntityDetailDto>(error);
        }

        try
        {
            var entity = MapEntity(request, id: 0, isActive: true);
            var phones = MapPhones(request.Phones);
            var id = category == "N"
                ? await entities.InsertAsync(entity, MapNatural(request), null, phones)
                : await entities.InsertAsync(entity, null, MapLegal(request), phones);

            return await GetByIdAsync(id);
        }
        catch (SqlException exception) when (DbErrors.TryMap(exception, out var mapped))
        {
            return Result.Failure<EntityDetailDto>(mapped);
        }
    }

    public async Task<Result> UpdateAsync(int id, EntityUpdateDto request)
    {
        var (error, category) = await ValidateAsync(request);
        if (error is not null)
        {
            return Result.Failure(error);
        }

        try
        {
            var entity = MapEntity(request, id, request.IsActive!.Value);
            var phones = MapPhones(request.Phones);
            if (category == "N")
            {
                await entities.UpdateAsync(entity, MapNatural(request), null, phones);
            }
            else
            {
                await entities.UpdateAsync(entity, null, MapLegal(request), phones);
            }

            return Result.Success();
        }
        catch (SqlException exception) when (DbErrors.TryMap(exception, out var mapped))
        {
            return Result.Failure(mapped);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            await entities.DeleteAsync(id);
            return Result.Success();
        }
        catch (SqlException exception) when (DbErrors.TryMap(exception, out var mapped))
        {
            return Result.Failure(mapped);
        }
    }

    /// <summary>
    /// Reglas de negocio antes de tocar la base: tipo de entidad válido,
    /// tipo de documento permitido para la categoría y datos del subtipo.
    /// Los procedimientos vuelven a validarlas como última línea de defensa.
    /// </summary>
    private async Task<(Error? Error, string? Category)> ValidateAsync(EntityCreateDto request)
    {
        if (request.Phones.Count > MaxPhones)
        {
            return (Error.Validation("entity.too_many_phones", $"Una entidad no puede tener más de {MaxPhones} teléfonos."), null);
        }

        var entityTypes = await catalogs.ListEntityTypesAsync();
        var entityType = entityTypes.FirstOrDefault(t => t.Id == request.EntityTypeId);
        if (entityType is null)
        {
            return (Error.Validation("entity.type_invalid", "El tipo de entidad no es válido."), null);
        }

        var documentTypes = await catalogs.ListDocumentTypesAsync(entityType.Category);
        if (documentTypes.All(d => d.Id != request.DocumentTypeId))
        {
            return (Error.Validation("document_type.not_allowed", "El tipo de documento no está permitido para este tipo de entidad."), null);
        }

        if (entityType.Category == "N")
        {
            if (string.IsNullOrWhiteSpace(request.Names) ||
                string.IsNullOrWhiteSpace(request.PaternalSurname) ||
                string.IsNullOrWhiteSpace(request.MaternalSurname) ||
                request.Age is null ||
                request.Sex is null)
            {
                return (Error.Validation("entity.natural_required", "La persona natural requiere nombres, apellido paterno, apellido materno, edad y sexo."), null);
            }
        }
        else if (string.IsNullOrWhiteSpace(request.LegalName))
        {
            return (Error.Validation("entity.legal_required", "La persona jurídica requiere razón social."), null);
        }

        return (null, entityType.Category);
    }

    private static Entity MapEntity(EntityCreateDto request, int id, bool isActive) => new()
    {
        Id = id,
        EntityTypeId = request.EntityTypeId,
        DocumentTypeId = request.DocumentTypeId,
        DocumentNumber = request.DocumentNumber.Trim(),
        Email = Normalize(request.Email),
        Address = Normalize(request.Address),
        IsActive = isActive
    };

    private static NaturalPerson MapNatural(EntityCreateDto request) => new()
    {
        Names = request.Names!.Trim(),
        PaternalSurname = request.PaternalSurname!.Trim(),
        MaternalSurname = request.MaternalSurname!.Trim(),
        Age = request.Age!.Value,
        Sex = request.Sex!
    };

    private static LegalPerson MapLegal(EntityCreateDto request) => new()
    {
        LegalName = request.LegalName!.Trim(),
        TradeName = Normalize(request.TradeName),
        ContactName = Normalize(request.ContactName)
    };

    private static List<Phone> MapPhones(List<PhoneDto> phones) =>
        phones.Select(p => new Phone { Number = p.Number.Trim(), Label = Normalize(p.Label) }).ToList();

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
