namespace OrbitShield.Infrastructure.Persistence;

public static class OracleConnectionStringFactory
{
    public static string Create(string? configuredConnectionString)
    {
        var oracleUrl = Environment.GetEnvironmentVariable("ORACLE_URL");
        var oracleUser = Environment.GetEnvironmentVariable("ORACLE_USER");
        var oraclePassword = Environment.GetEnvironmentVariable("ORACLE_PASSWORD");

        if (!string.IsNullOrWhiteSpace(oracleUrl)
            && !string.IsNullOrWhiteSpace(oracleUser)
            && !string.IsNullOrWhiteSpace(oraclePassword))
        {
            return $"User Id={oracleUser};Password={oraclePassword};Data Source={ConvertJdbcUrlToDataSource(oracleUrl)}";
        }

        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return configuredConnectionString;
        }

        throw new InvalidOperationException("Oracle connection settings are not configured.");
    }

    private static string ConvertJdbcUrlToDataSource(string jdbcUrl)
    {
        const string prefix = "jdbc:oracle:thin:@";
        var dataSource = jdbcUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? jdbcUrl[prefix.Length..]
            : jdbcUrl;

        var parts = dataSource.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 3
            ? $"{parts[0]}:{parts[1]}/{parts[2]}"
            : dataSource;
    }
}
