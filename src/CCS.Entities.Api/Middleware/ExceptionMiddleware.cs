using Microsoft.Data.SqlClient;

namespace CCS.Entities.Api.Middleware;

public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) : IMiddleware
{
    private const int SqlErrorConflict = 50003;
    private const int SqlErrorNotFound = 50010;
    private const int SqlErrorValidationMin = 50001;
    private const int SqlErrorValidationMax = 50099;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            SqlException { Number: SqlErrorConflict } =>
                (StatusCodes.Status409Conflict, "Conflicto de datos"),
            SqlException { Number: SqlErrorNotFound } =>
                (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            SqlException { Number: >= SqlErrorValidationMin and <= SqlErrorValidationMax } =>
                (StatusCodes.Status400BadRequest, "Error de validación"),
            SqlException =>
                (StatusCodes.Status500InternalServerError, "Error de base de datos"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Error inesperado")
        };

        logger.LogError(exception,
            "Error al procesar {Method} {Path}: {Message}",
            context.Request.Method, context.Request.Path, exception.Message);

        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "Ocurrió un error inesperado. Consulte el log para más detalle."
            : exception.Message;

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new
        {
            status = statusCode,
            title,
            detail
        });
    }
}
