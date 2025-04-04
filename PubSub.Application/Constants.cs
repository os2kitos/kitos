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
                public const string CurrentEnvironment = "ASPNETCORE_ENVIRONMENT";
                public const string Production = "Production";
            }

            public class MessageBus
            {
                public const string ConfigSection = "RabbitMQ";
                public const string HostName = "HostName";
                public const string User = "RABBIT_MQ_USER";
                public const string Password = "RABBIT_MQ_PASSWORD";
            }

            public class CallbackAuthentication
            {
                public const string PubSubApiKey = "PUBSUB_API_KEY";
            }

            public class Certificate
            {
                public const string CertPassword = "CERT_PASSWORD";
            }
        }
    }
}
