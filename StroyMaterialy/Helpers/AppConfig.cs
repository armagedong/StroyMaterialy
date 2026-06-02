using Microsoft.Extensions.Configuration;

namespace StroyMaterialy.Helpers;

public static class AppConfig
{
    private static IConfiguration? _configuration;

    public static void Initialize()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public static string ConnectionString =>
        _configuration?["ConnectionStrings:Postgres"]
        ?? "Host=localhost;Port=5432;Database=stroymaterialy;Username=postgres;Password=postgres";

    public static string ImportPath
    {
        get
        {
            var configured = _configuration?["ImportPath"] ?? "..\\импорт";
            var path = Path.IsPathRooted(configured)
                ? configured
                : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configured));

            if (!Directory.Exists(path))
            {
                var alt = Path.GetFullPath(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..", "..", "..", "импорт"));
                if (Directory.Exists(alt))
                    return alt;
            }

            return path;
        }
    }
}
