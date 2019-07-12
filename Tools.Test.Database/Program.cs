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

                case CliTargets.CreateTestUser:
                    Console.WriteLine("Expecting the following arguments: <connectionString> <email> <password> <role>");
                    var createUserArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        Email = GetArgument(additionalArgs, 1),
                        Password = GetArgument(additionalArgs, 2),
                        Role = GetArgument(additionalArgs, 3),
                    };

                    return new CreateKitosUserTask(createUserArgs.ConnectionString, createUserArgs.Email, createUserArgs.Password, createUserArgs.Role);

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
                    var createSystemArgs = new
                    {
                        ConnectionString = GetArgument(additionalArgs, 0),
                        ItSystemName = GetArgument(additionalArgs, 1)
                    };

                    FailOnConnectionToProd(createSystemArgs.ConnectionString);
                    return new CreateItSystemTask(createSystemArgs.ConnectionString, createSystemArgs.ItSystemName);
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
    }
}
