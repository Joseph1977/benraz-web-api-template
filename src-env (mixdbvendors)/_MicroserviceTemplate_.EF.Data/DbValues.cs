namespace _MicroserviceTemplate_.EF.Data;

public static class DbValues
{
    private static readonly Dictionary<DatabaseProvider, DbValueProperties> ProviderMappings =
        new()
        {
            [DatabaseProvider.SqlServer] = new DbValueProperties(
                Guid: "uniqueidentifier",
                Utc: "GETUTCDATE()"
            ),
            [DatabaseProvider.PostgreSql] = new DbValueProperties(
                Guid: "uuid",
                Utc: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"
            )
        };

    public static string Guid => ProviderMappings[GlobalConfig.DatabaseProvider].Guid;
    public static string Utc => ProviderMappings[GlobalConfig.DatabaseProvider].Utc;
}

public record DbValueProperties(string Guid, string Utc);