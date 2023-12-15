using System;

namespace Infrastructure.DataAccess.Tools
{
    public class ConnectionStringTools
    {
        public static string GetConnectionString(string dbName)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[dbName]?.ConnectionString ?? dbName;
            if (!connectionString.StartsWith("base64:")) 
                return connectionString;
            
            var base64EncodedString = connectionString.Substring("base64:".Length);
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedString);
            connectionString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            return connectionString;
        }
    }
}
