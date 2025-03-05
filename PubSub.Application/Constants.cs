namespace PubSub.Application
{
    public class Constants
    {
        public class Config
        {
            public class Validation
            {
                public const string Url = "JwtValidation:ApiUrl";
                public const string Endpoint = "JwtValidation:ValidationEndpoint";
            }

            public class Environment
            {
                public const string CurrentName = "ASPNETCORE_ENVIRONMENT";
                public const string Production = "Production";
            }
        }
    }
}
