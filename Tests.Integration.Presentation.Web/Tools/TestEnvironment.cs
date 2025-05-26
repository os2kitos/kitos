using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Interceptors;
using Moq;
using Tests.Integration.Presentation.Web.Tools.Model;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class TestEnvironment
    {
        private static readonly IReadOnlyDictionary<OrganizationRole, KitosCredentials> UsersFromEnvironment;
        private static readonly IReadOnlyDictionary<OrganizationRole, KitosCredentials> ApiUsersFromEnvironment;
        private static readonly KitosTestEnvironment ActiveEnvironment;
        private static readonly string DefaultUserPassword;
        public const int DefaultOrganizationId = 1;
        public const string DefaultOrganizationName = "Fælles Kommune";
        public const int SecondOrganizationId = 2;
        public const int DefaultUserId = 1;
        private static readonly string ConnectionString;

        static TestEnvironment()
        {
            //Fake the interception for EF in the text context
            DbInterception.Add(new EFEntityInterceptor(() => new OperationClock(), () => Maybe<ActiveUserIdContext>.None, () => Mock.Of<IFallbackUserResolver>(x => x.Resolve() == new User() { Id = DefaultUserId })));

            var testEnvironment = GetEnvironmentVariable("KitosTestEnvironment", false);
            if (string.IsNullOrWhiteSpace(testEnvironment))
            {
                ActiveEnvironment = KitosTestEnvironment.Local;
            }
            else
            {
                ActiveEnvironment = (KitosTestEnvironment)Enum.Parse(typeof(KitosTestEnvironment), testEnvironment, true);
            }

            if (ActiveEnvironment == KitosTestEnvironment.Local)
            {
                //Expecting the following users to be available to local testing
                Console.Out.WriteLine("Running locally. Loading all configuration in-line");
                const string localDevUserPassword = "localNoSecret";
                DefaultUserPassword = "arne123";
                ConnectionString = @"Server=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Kitos;MultipleActiveResultSets=True";
                UsersFromEnvironment = new Dictionary<OrganizationRole, KitosCredentials>
                {
                    {
                        OrganizationRole.User,
                        new KitosCredentials(
                            "local-regular-user@kitos.dk",
                            localDevUserPassword)
                    },
                    {
                        OrganizationRole.LocalAdmin,
                        new KitosCredentials(
                            "local-local-admin-user@kitos.dk",
                            localDevUserPassword)
                    },
                    {
                        OrganizationRole.GlobalAdmin,
                        new KitosCredentials(
                            "local-global-admin-user@kitos.dk",
                            localDevUserPassword)
                    }
                };
                ApiUsersFromEnvironment = new Dictionary<OrganizationRole, KitosCredentials>
                {
                    {
                        OrganizationRole.User,
                        new KitosCredentials(
                            "local-api-user@kitos.dk",
                            localDevUserPassword)
                    },
                    {
                        OrganizationRole.GlobalAdmin,
                        new KitosCredentials(
                            "local-api-global-admin-user@kitos.dk",
                            localDevUserPassword)
                    }
                };

            }
            else
            {
                //Loading users from environment
                Console.Out.WriteLine("Tests running towards remote target. Loading configuration from environment.");
                DefaultUserPassword = GetEnvironmentVariable("DefaultUserPassword");
                ConnectionString = GetEnvironmentVariable("KitosDbConnectionStringForTeamCity");
                UsersFromEnvironment = new Dictionary<OrganizationRole, KitosCredentials>
                {
                    {OrganizationRole.User, LoadUserFromEnvironment(OrganizationRole.User)},
                    {OrganizationRole.LocalAdmin, LoadUserFromEnvironment(OrganizationRole.LocalAdmin)},
                    {OrganizationRole.GlobalAdmin, LoadUserFromEnvironment(OrganizationRole.GlobalAdmin)}
                };
                ApiUsersFromEnvironment = new Dictionary<OrganizationRole, KitosCredentials>
                {

                    {OrganizationRole.User, LoadUserFromEnvironment(OrganizationRole.User, true)},
                    {OrganizationRole.GlobalAdmin, LoadUserFromEnvironment(OrganizationRole.GlobalAdmin, true)}
                };
            }
        }

        private static KitosCredentials LoadUserFromEnvironment(OrganizationRole role, bool apiAccess = false)
        {
            var suffix = string.Empty;
            switch (role)
            {
                case OrganizationRole.User when apiAccess:
                    suffix = "ApiUser";
                    break;
                case OrganizationRole.User:
                    suffix = "NormalUser";
                    break;
                case OrganizationRole.LocalAdmin:
                    suffix = "LocalAdmin";
                    break;
                case OrganizationRole.GlobalAdmin when apiAccess:
                    suffix = "ApiGlobalAdmin";
                    break;
                case OrganizationRole.GlobalAdmin:
                    suffix = "GlobalAdmin";
                    break;
                default:
                    throw new NotSupportedException($"{role} Not mapped in environment loader:{nameof(LoadUserFromEnvironment)}");
            }

            var username = GetEnvironmentVariable($"TestUser{suffix}");
            var password = GetEnvironmentVariable($"TestUser{suffix}Pw");
            return new KitosCredentials(username, password);
        }
        public static KitosContext GetDatabase()
        {
            return new KitosContext(ConnectionString);
        }

        private static string GetEnvironmentVariable(string name, bool mandatory = true, string defaultValue = null)
        {
            var variableName = name;

            var variable = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
            if (string.IsNullOrWhiteSpace(variable))
            {
                if (mandatory)
                {
                    throw new ArgumentException($"Error: No environment variable value found for mandatory variable '{variable}'");
                }

                return defaultValue;
            }
            return variable;
        }

        public static KitosCredentials GetCredentials(OrganizationRole role, bool apiAccess = false)
        {
            var userEnvironment = apiAccess ? ApiUsersFromEnvironment : UsersFromEnvironment;

            if (userEnvironment.TryGetValue(role, out var credentials))
            {
                return credentials;
            }
            throw new ArgumentNullException($"No environment {(apiAccess ? "api " : "")}user configured for role:{role:G}");
        }

        public static string GetBaseUrl()
        {
            switch (ActiveEnvironment)
            {
                case KitosTestEnvironment.Local:
                    return "https://localhost:44300";
                case KitosTestEnvironment.Integration:
                    return $"https://{GetEnvironmentVariable("KitosHostName")}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Uri CreateUrl(string pathAndQuery)
        {
            return new Uri($"{GetBaseUrl()}/{pathAndQuery.TrimStart('/')}");
        }

        public static string GetDefaultUserPassword()
        {
            return DefaultUserPassword;
        }
    }
}
