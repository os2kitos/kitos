using System.Linq;

namespace Infrastructure.DataAccess.Tools
{
    public static class SqlMigrationScriptRepository
    {
        public static string GetResourceName(string nameContent)
        {
            return typeof(SqlMigrationScriptRepository).Assembly.GetManifestResourceNames().First(x => x.Contains(nameContent));
        }
    }
}
