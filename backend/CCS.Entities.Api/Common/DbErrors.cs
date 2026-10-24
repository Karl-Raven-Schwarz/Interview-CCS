using Microsoft.Data.SqlClient;

namespace CCS.Entities.Api.Common;

public static class DbErrors
{
    // THROW definidos en db/02_procedures.sql
    public const int TypeInvalid = 50001;
    public const int DocumentNotAllowed = 50002;
    public const int DuplicateDocument = 50003;
    public const int NaturalDataRequired = 50004;
    public const int LegalDataRequired = 50005;
    public const int EntityNotFound = 50010;
    public const int DocumentInvalidFormat = 50011;

    public const int ThrowMin = TypeInvalid;
    public const int ThrowMax = 50099;

    public const int CheckViolation = 547;
    public const int UniqueViolation = 2627;
    public const int DuplicateKey = 2601;

    public static bool IsThrowRange(int number) => number is >= ThrowMin and <= ThrowMax;

    public static bool TryMap(SqlException exception, out Error error)
    {
        switch (exception.Number)
        {
            case EntityNotFound:
                error = Error.NotFound("entity.not_found", exception.Message);
                return true;

            case DuplicateDocument:
                error = Error.Conflict("entity.duplicate_document", exception.Message);
                return true;

            case UniqueViolation:
            case DuplicateKey:
                // El mensaje de SQL nombra el índice: se usa mensaje propio.
                error = Error.Conflict("entity.duplicate_document", "Ya existe una entidad registrada con ese tipo y número de documento.");
                return true;

            case TypeInvalid:
                error = Error.Validation("entity.type_invalid", exception.Message);
                return true;

            case DocumentNotAllowed:
                error = Error.Validation("document_type.not_allowed", exception.Message);
                return true;

            case DocumentInvalidFormat:
                error = Error.Validation("document.invalid_format", exception.Message);
                return true;

            case NaturalDataRequired:
                error = Error.Validation("entity.natural_required", exception.Message);
                return true;

            case LegalDataRequired:
                error = Error.Validation("entity.legal_required", exception.Message);
                return true;

            case CheckViolation:
                // El mensaje de SQL nombra el constraint: se usa mensaje propio.
                error = Error.Validation("db.check_violation", "Alguno de los datos enviados no cumple las reglas de validación de la base de datos.");
                return true;

            default:
                error = Error.None;
                return false;
        }
    }
}
