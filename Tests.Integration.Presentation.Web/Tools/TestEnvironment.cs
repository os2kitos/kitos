using System;
using System.Collections.Generic;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Tests.Integration.Presentation.Web.Tools.Model;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class TestEnvironment
    {
        private static readonly IReadOnlyDictionary<OrganizationRole, KitosCredentials> UsersFromEnvironment;
        private static readonly IReadOnlyDictionary<OrganizationRole, KitosCredentials> ApiUsersFromEnvironment;
        private static readonly IConfiguration AppSettings = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();
        private static readonly KitosTestEnvironment ActiveEnvironment;
        private static readonly string DefaultUserPassword;
        public const int DefaultOrganizationId = 1;
        public const string DefaultOrganizationName = "Fælles Kommune";
        public const int SecondOrganizationId = 2;
        public const int DefaultUserId = 1;
        private static readonly string ConnectionString;
        private static readonly string DatabaseProvider;
        private static readonly string BaseUrl;

        static TestEnvironment()
        {
            // Configure Npgsql timestamp compatibility as early as possible in the test process.
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var testEnvironment = GetEnvironmentVariable("KitosTestEnvironment", false, allowAppSettingsFallback: false);
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
                DatabaseProvider = GetEnvironmentVariable("KitosDbProvider", false,
                    GetEnvironmentVariable("Database__Provider", false, "PostgreSql"));
                ConnectionString = ResolveLocalConnectionString();
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

                Console.Out.WriteLine($"[TestEnvironment] ActiveEnvironment={ActiveEnvironment}, DatabaseProvider={DatabaseProvider}");
                BaseUrl = "https://localhost:44300";
            }
            else
            {
                //Loading users from environment
                Console.Out.WriteLine("Tests running towards remote target. Loading configuration from environment.");
                DefaultUserPassword = GetEnvironmentVariable("DefaultUserPassword");
                DatabaseProvider = GetEnvironmentVariable("KitosDbProvider", false,
                    GetEnvironmentVariable("Database__Provider", false, "PostgreSql"));
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

                BaseUrl = $"https://{GetEnvironmentVariable("KitosHostName")}";
                Console.Out.WriteLine($"[TestEnvironment] ActiveEnvironment={ActiveEnvironment}, DatabaseProvider={DatabaseProvider}");
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
            var optionsBuilder = new DbContextOptionsBuilder<KitosContext>()
                .UseLazyLoadingProxies();

            var pgCsb = new NpgsqlConnectionStringBuilder(ConnectionString) { SearchPath = "dbo,public" };
            optionsBuilder.UseNpgsql(pgCsb.ConnectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));

            var options = optionsBuilder.Options;
            return new KitosContext(options);
        }


        private static string ResolveLocalConnectionString()
        {
            return GetEnvironmentVariable("ConnectionStrings__KitosContext", false,
                @"Host=localhost;Port=5432;Database=kitos;Username=postgres;Password=localNoSecret");
        }

        private static string GetEnvironmentVariable(string name, bool mandatory = true, string? defaultValue = null, bool allowAppSettingsFallback = true)
        {
            var variableName = name;

            var variable = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
            var source = "process-env";
            if (allowAppSettingsFallback && string.IsNullOrWhiteSpace(variable))
            {
                // Fall back to appsettings.json — double underscore (__) maps to colon (:) for hierarchy
                var configKey = name.Replace("__", ":");
                variable = AppSettings[configKey];
                source = "appsettings.json";
            }

            if (string.IsNullOrWhiteSpace(variable))
            {
                if (mandatory)
                {
                    Console.Out.WriteLine($"[TestEnvironment] '{name}' missing (mandatory). allowAppSettingsFallback={allowAppSettingsFallback}");
                    throw new ArgumentException($"Error: No environment variable value found for mandatory variable '{name}'");
                }

                Console.Out.WriteLine($"[TestEnvironment] '{name}' not set. Using default. allowAppSettingsFallback={allowAppSettingsFallback}");
                return defaultValue!;
            }

            Console.Out.WriteLine($"[TestEnvironment] '{name}' loaded from {source}. allowAppSettingsFallback={allowAppSettingsFallback}");
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
                    return BaseUrl;
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
