using System.Data;
using CCS.Entities.Api.Data;
using CCS.Entities.Api.Models;
using Dapper;

namespace CCS.Entities.Api.Repositories;

public class UserRepository(DapperContext context)
{
    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        using var connection = context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<AppUser>(
            "sp_Auth_GetUser",
            new { Username = username },
            commandType: CommandType.StoredProcedure);
    }
}
