using Npgsql;
using StroyMaterialy.Helpers;

namespace StroyMaterialy.Services;

public static class DatabaseService
{
    public static NpgsqlConnection CreateConnection() => new(AppConfig.ConnectionString);

    public static async Task EnsureSchemaAsync()
    {
        var scriptPath = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "database", "init.sql"));

        if (!File.Exists(scriptPath))
        {
            scriptPath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "database", "init.sql"));
        }

        if (!File.Exists(scriptPath))
            return;

        var sql = await File.ReadAllTextAsync(scriptPath);
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<bool> HasDataAsync()
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM products", conn);
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return count > 0;
    }
}
