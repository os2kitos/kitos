using System;
using System.Linq;
using Tools.Test.Database.Model.Tasks;

namespace Tools.Test.Database
{
    class Program
    {
        static int Main(string[] args)
        {
            var task = args[0];
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

        private static IDatabaseTask CreateTask(string task, string[] additionalArgs)
        {
            switch (task)
            {
                case "CreateCleanDatabase":
                    var connectionString = additionalArgs[0].Trim('"');
                    return new DropDatabaseTask(connectionString);
                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task, "Unknown task provided");
            }
        }
    }
}
