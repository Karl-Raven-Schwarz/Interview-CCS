namespace CCS.Entities.Api.Common;

public enum ErrorType
{
    /// <summary>Error no esperado (500 Internal Server Error).</summary>
    Failure = 0,

    /// <summary>Datos inválidos (400 Bad Request).</summary>
    Validation = 1,

    /// <summary>El recurso no existe (404 Not Found).</summary>
    NotFound = 2,

    /// <summary>Conflicto con el estado actual (409 Conflict), p. ej. documento duplicado.</summary>
    Conflict = 3,

    /// <summary>Credenciales inválidas o falta autorización (401 Unauthorized).</summary>
    Unauthorized = 4
}
