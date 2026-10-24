using Microsoft.AspNetCore.Mvc;

namespace CCS.Entities.Api.Common;

/// <summary>Traduce un <see cref="Result"/> fallido a la respuesta HTTP que le corresponde.</summary>
public static class ResultExtensions
{
    public static ObjectResult ToProblem(this Result result) => ToProblem(result.Error);

    public static ObjectResult ToProblem<T>(this Result<T> result) => ToProblem(result.Error);

    /// <summary>Código y título HTTP de cada tipo de error.</summary>
    public static (int Status, string Title) Describe(ErrorType type) => type switch
    {
        ErrorType.Validation => (StatusCodes.Status400BadRequest, "Error de validación"),
        ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
        ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflicto de datos"),
        ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "No autorizado"),
        _ => (StatusCodes.Status500InternalServerError, "Error inesperado")
    };

    private static ObjectResult ToProblem(Error error)
    {
        var (status, title) = Describe(error.Type);
        return new ObjectResult(new { status, title, detail = error.Message, code = error.Code })
        {
            StatusCode = status
        };
    }
}
