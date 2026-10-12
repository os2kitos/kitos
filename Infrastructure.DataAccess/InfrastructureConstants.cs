namespace Infrastructure.DataAccess
{
    internal static class InfrastructureConstants
    {
        internal const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";

        internal const string SqlServerMaxTextType = "nvarchar(max)";
        internal const string PostgreSqlMaxTextType = "text";

        internal const string SqlServerUuidType = "uniqueidentifier";
        internal const string PostgreSqlUuidType = "uuid";

        internal const string SqlServerIntType = "int";
        internal const string PostgreSqlIntType = "integer";

        internal const string SqlServerDateTimeType = "datetime2";
        internal const string PostgreSqlDateTimeType = "timestamp without time zone";
    }
}
