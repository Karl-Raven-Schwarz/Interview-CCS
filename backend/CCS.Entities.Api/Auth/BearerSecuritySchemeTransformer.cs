using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CCS.Entities.Api.Auth;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var components = document.Components ??= new OpenApiComponents();
        var securitySchemes = components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        securitySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Pegue aquí el token obtenido en POST /api/auth/login."
        };

        return Task.CompletedTask;
    }
}
