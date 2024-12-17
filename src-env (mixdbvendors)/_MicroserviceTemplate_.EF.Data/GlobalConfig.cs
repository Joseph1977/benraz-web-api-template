namespace _MicroserviceTemplate_.EF.Data;

public static class GlobalConfig
{
    public static DatabaseProvider DatabaseProvider { get; set; }
}

public enum DatabaseProvider
{
    SqlServer = 1,
    PostgreSql = 2
}