using System.Data;
using Microsoft.Data.SqlClient;

namespace CCS.Entities.Api.Data;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CCSDb")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'CCSDb' en appsettings.json.");
    }

    /// <summary>Crea una conexión nueva. El llamador es responsable de cerrarla.</summary>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
