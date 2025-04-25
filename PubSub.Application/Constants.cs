namespace PubSub.Application
{
    public static class Constants
    {
        public static class Config
        {
            public static class Validation
            {
                public const string Url = "JwtValidation:ApiUrl";
                public const string Endpoint = "JwtValidation:ValidationEndpoint";
                public const string CanPublishPolicy = "CanPublishPolicy";
                public const string CanSubscribePolicy = "CanSubscribePolicy";
            }

            public static class Environment
            {
                public const string CurrentEnvironment = "ASPNETCORE_ENVIRONMENT";
                public const string Production = "Production";
            }

            public static class MessageBus
            {
                public const string ConfigSection = "RabbitMQ";
                public const string HostName = "HostName";
                public const string User = "RABBIT_MQ_USER";
                public const string Password = "RABBIT_MQ_PASSWORD";
            }

            public static class CallbackAuthentication
            {
                public const string PubSubApiKey = "PUBSUB_API_KEY";
            }

            public static class Certificate
            {
                public const string CertPassword = "CERT_PASSWORD";
            }
        }

        public static class Claims
        {
            public const string CanPublish = "CanPublish";
            public const string CanSubscribe = "CanSubscribe";
            public const string True = "true";
        }
    }
}
