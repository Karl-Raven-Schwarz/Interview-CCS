using CCS.Entities.Api.Common;
using Microsoft.Data.SqlClient;

namespace CCS.Entities.Api.Middleware;

public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) : IMiddleware
{
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
        logger.LogError(exception,
            "Error al procesar {Method} {Path}: {Message}",
            context.Request.Method, context.Request.Path, exception.Message);

        // Los errores de negocio de la base se traducen a su respuesta;
        // cualquier otro no se filtra al cliente.
        var error = exception is SqlException sql && DbErrors.TryMap(sql, out var mapped)
            ? mapped
            : Error.Failure("server.error", "Ocurrió un error inesperado. Consulte el log para más detalle.");

        var (status, title) = ResultExtensions.Describe(error.Type);
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new
        {
            status,
            title,
            detail = error.Message,
            code = error.Code
        });
    }
}
