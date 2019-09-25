using System;
using System.Linq;
using Tools.Test.Database.Model.Cli;
using Tools.Test.Database.Model.Environment;
using Tools.Test.Database.Model.Tasks;

namespace Tools.Test.Database
{
    class Program
    {
        static int Main(string[] args)
        {
            var task = GetArgument(args, 0);
            var additionalArgs = args.Skip(1).ToArray();

            var databaseTask = CreateTask(task, additionalArgs);
            var taskName = $"{task}(implemented in {databaseTask.GetType().Name})";

            try
            {
                Console.WriteLine($"Executing {taskName}");
                var success = databaseTask.Execute();
                if (success == false)
                {
                    Console.WriteLine($"Failed to execute {taskName}");
                    return -1;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine($"Executed {taskName} with success");
            return 0;
        }

        private static DatabaseTask CreateTask(string task, string[] additionalArgs)
        {
            switch (task)
            {
                case CliTargets.DropDatabase:
                    Console.WriteLine("Expecting the following arguments: <connectionString>");

                    var connectionString = GetArgument(additionalArgs, 0);
                    FailOnConnectionToProd(connectionString);
                    return new DropDatabaseTask(connectionString);

                case CliTargets.CreateOrganization:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <organizationType> <organizationName>");
                    var createOrganizationArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        OrganizationType = GetIntegerArgument(additionalArgs, 1),
                        OrganizationName = GetArgument(additionalArgs, 2)
                    };

                    FailOnConnectionToProd(createOrganizationArgs.ConnectionString);
                    return new CreateOrganizationTask(createOrganizationArgs.ConnectionString, createOrganizationArgs.OrganizationType, createOrganizationArgs.OrganizationName);

                case CliTargets.CreateTestUser:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <email> <password> <role> <organizationNames>");
                    var createUserArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        Email = GetArgument(additionalArgs, 1),
                        Password = GetArgument(additionalArgs, 2),
                        Role = GetArgument(additionalArgs, 3),
                        OrganizationNames = GetArgument(additionalArgs, 4)
                    };

                    FailOnConnectionToProd(createUserArgs.ConnectionString);
                    return new CreateKitosUserTask(createUserArgs.ConnectionString, createUserArgs.Email, createUserArgs.Password, createUserArgs.Role, createUserArgs.OrganizationNames);

                case CliTargets.CreateApiTestUser:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <email> <password> <role> <organizationNames>");
                    var createApiUserArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        Email = GetArgument(additionalArgs, 1),
                        Password = GetArgument(additionalArgs, 2),
                        Role = GetArgument(additionalArgs, 3),
                        OrganizationNames = GetArgument(additionalArgs, 4)
                    };

                    FailOnConnectionToProd(createApiUserArgs.ConnectionString);
                    return new CreateKitosUserTask(createApiUserArgs.ConnectionString, createApiUserArgs.Email, createApiUserArgs.Password, createApiUserArgs.Role, createApiUserArgs.OrganizationNames, true);

                case CliTargets.EnableAllOptions:
                    Console.WriteLine("Expecting the following arguments: <connectionString>");
                    var enableAllArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                    };

                    FailOnConnectionToProd(enableAllArgs.ConnectionString);
                    return new EnableAllOptionsTask(enableAllArgs.ConnectionString);

                case CliTargets.CreateItSystem:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <it_system_name>");
                    var createItSystemArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        ItSystemName = GetArgument(additionalArgs, 1),
                        OrganizationName = GetArgument(additionalArgs, 2)
                    };

                    FailOnConnectionToProd(createItSystemArgs.ConnectionString);
                    return new CreateItSystemTask(createItSystemArgs.ConnectionString, createItSystemArgs.ItSystemName, createItSystemArgs.OrganizationName, null);

                case CliTargets.CreateItSystemWithParent:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <it_system_name>");
                    var createItSystemWithParentArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        ItSystemName = GetArgument(additionalArgs, 1),
                        OrganizationName = GetArgument(additionalArgs, 2),
                        ParentId = GetIntegerArgument(additionalArgs, 3)
                    };

                    FailOnConnectionToProd(createItSystemWithParentArgs.ConnectionString);
                    return new CreateItSystemTask(createItSystemWithParentArgs.ConnectionString, createItSystemWithParentArgs.ItSystemName, createItSystemWithParentArgs.OrganizationName, createItSystemWithParentArgs.ParentId);

                case CliTargets.CreateItContract:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <it_contract_name>");
                    var createContractArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        ItSystemName = GetArgument(additionalArgs, 1)
                    };

                    FailOnConnectionToProd(createContractArgs.ConnectionString);
                    return new CreateItContractTask(createContractArgs.ConnectionString, createContractArgs.ItSystemName);

                case CliTargets.CreateTaskRef:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <organizationName>");
                    var createTaskRefArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        OrganizationName = GetArgument(additionalArgs, 1)
                    };

                    FailOnConnectionToProd(createTaskRefArgs.ConnectionString);
                    return new CreateTaskRefTask(createTaskRefArgs.ConnectionString, createTaskRefArgs.OrganizationName);

                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task, "Unknown task provided");
            }
        }

        private static void FailOnConnectionToProd(string connectionString)
        {
            if (Production.ContainsProductionIp(connectionString))
            {
                throw new NotSupportedException("This operation is not allowed in prod.");
            }
        }

        private static string GetArgument(string[] additionalArgs, int index, bool trimEnclosingQuotes = true)
        {
            var arg = additionalArgs[index];
            if (trimEnclosingQuotes)
            {
                arg = arg.Trim('"');
            }
            return arg;
        }

        private static int GetIntegerArgument(string[] additionalArgs, int index, bool trimEnclosingQuotes = true)
        {
            var arg = GetArgument(additionalArgs, index, trimEnclosingQuotes);
            if (int.TryParse(arg, out int res))
            {
                return res;
            }
            else
            {
                throw new ArgumentException($"argument at index {index} must be an integer");
            }
        }

    }
}
