﻿namespace PubSub.Application.Api
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
                public const string CertFilePath = "/etc/ssl/certs/kitos-pubsub.pfx";
            }
        }

        public static class Claims
        {
            public const string CanPublish = "CanPublish";
            public const string CanSubscribe = "CanSubscribe";
            public const string True = "true";
        }

        public static class ApiVersion
        {
            public const int Version1 = 1;
        }
    }
}
