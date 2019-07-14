using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools.Model;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class TestEnvironment
    {
        private static readonly IReadOnlyDictionary<OrganizationRole, KitosCredentials> UsersFromEnvironment;
        private static readonly KitosTestEnvironment ActiveEnvironment;

        static TestEnvironment()
        {
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
                UsersFromEnvironment = new Dictionary<OrganizationRole, KitosCredentials>
                {
                    {
                        OrganizationRole.User,
                        new KitosCredentials(
                            "local-regular-user@strongminds.dk", 
                            "localNoSecret",
                            OrganizationRole.User)
                    },
                    {
                        OrganizationRole.LocalAdmin,
                        new KitosCredentials(
                            "local-local-admin-user@strongminds.dk", 
                            "localNoSecret",
                            OrganizationRole.LocalAdmin)
                    },
                    {
                        OrganizationRole.GlobalAdmin,
                        new KitosCredentials(
                            "local-global-admin-user@strongminds.dk", 
                            "localNoSecret",
                            OrganizationRole.GlobalAdmin)
                    }
                };
            }
            else
            {
                //Loading users from environment
                Console.Out.WriteLine("Tests running towards remote target. Loading configuration from environment.");
                UsersFromEnvironment = new Dictionary<OrganizationRole, KitosCredentials>
                {
                    {OrganizationRole.User, LoadUserFromEnvironment(OrganizationRole.User)},
                    {OrganizationRole.LocalAdmin, LoadUserFromEnvironment(OrganizationRole.LocalAdmin)},
                    {OrganizationRole.GlobalAdmin, LoadUserFromEnvironment(OrganizationRole.GlobalAdmin)}
                };
            }
        }

        private static KitosCredentials LoadUserFromEnvironment(OrganizationRole role)
        {
            var suffix = string.Empty;
            switch (role)
            {
                case OrganizationRole.User:
                    suffix = "NormalUser";
                    break;
                case OrganizationRole.LocalAdmin:
                    suffix = "LocalAdmin";
                    break;
                case OrganizationRole.GlobalAdmin:
                    suffix = "GlobalAdmin";
                    break;
                default:
                    throw new NotSupportedException($"{role} Not mapped in environment loader:{nameof(LoadUserFromEnvironment)}");
            }

            var username = GetEnvironmentVariable($"TestUser{suffix}");
            var password = GetEnvironmentVariable($"TestUser{suffix}Pw");
            return new KitosCredentials(username, password, role);
        }

        private static string GetEnvironmentVariable(string name, bool mandatory = true, string defaultValue = null)
        {
            var variableName = name;
            Console.Out.WriteLine($"Reading '{variableName}' from environment");

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

        public static KitosCredentials GetCredentials(OrganizationRole role)
        {
            if (UsersFromEnvironment.TryGetValue(role, out var credentials))
            {
                return credentials;
            }
            throw new ArgumentNullException($"No environment user configured for role:{role:G}");
        }
    }
}
