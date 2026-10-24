namespace CCS.Entities.Api.Common;

/// <summary>
/// Error de dominio: un código estable (para el cliente) y un mensaje legible.
/// </summary>
/// <param name="Code">Código corto y estable, p. ej. "entity.not_found".</param>
/// <param name="Message">Mensaje descriptivo para mostrar al usuario.</param>
/// <param name="Type">Clasificación del error (define el código HTTP).</param>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>Representa la ausencia de error (resultado exitoso).</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
}
