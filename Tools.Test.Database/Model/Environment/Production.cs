using System;

namespace Tools.Test.Database.Model.Environment
{
    public static class Production
    {
        private const string DatabaseIp = "10.2.23.20";

        public static bool ContainsProductionIp(string source)
        {
            return source.IndexOf(DatabaseIp, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}
