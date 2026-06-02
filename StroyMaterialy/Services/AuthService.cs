using Npgsql;
using StroyMaterialy.Models;

namespace StroyMaterialy.Services;

public static class AuthService
{
    public static async Task<UserAccount?> AuthenticateAsync(string login, string password)
    {
        const string sql = """
            SELECT u.id, u.full_name, u.login, u.password, r.name
            FROM users u
            JOIN roles r ON r.id = u.role_id
            WHERE LOWER(u.login) = LOWER(@login) AND u.password = @password
            """;

        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("login", login.Trim());
        cmd.Parameters.AddWithValue("password", password);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        var roleName = reader.GetString(4);
        return new UserAccount
        {
            Id = reader.GetInt32(0),
            FullName = reader.GetString(1),
            Login = reader.GetString(2),
            Password = reader.GetString(3),
            RoleName = roleName,
            RoleType = UserAccount.ParseRole(roleName)
        };
    }
}
