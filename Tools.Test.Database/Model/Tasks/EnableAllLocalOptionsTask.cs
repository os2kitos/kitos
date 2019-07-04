using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.DomainModel;

namespace Tools.Test.Database.Model.Tasks
{
    public class EnableAllLocalOptionsTask : DatabaseTask
    {
        public EnableAllLocalOptionsTask(string connectionString) : base(connectionString)
        {
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var optionTypes = LoadAllOptionTypes();

                foreach (var optionType in optionTypes)
                {
                    Console.Out.WriteLine("Enabling all options of type:" + optionType.Name);
                    var dbSet = context.Set(optionType).Cast<Entity>();
                    EnableAllLocalOptions(dbSet);
                    context.SaveChanges();
                }
            }

            return true;
        }

        private static IEnumerable<Type> LoadAllOptionTypes()
        {
            var optionTypes =
                typeof(LocalOptionEntity<>)
                    .Assembly
                    .GetExportedTypes()
                    .Where(x => x.IsAbstract == false && IsOptionType(x))
                    .ToList();
            return optionTypes;
        }

        private static bool IsOptionType(Type type)
        {
            var baseType = type.BaseType;

            return
                baseType?.IsGenericType == true
                && baseType.GetGenericTypeDefinition() == typeof(LocalOptionEntity<>);
        }

        private static void EnableAllLocalOptions(IEnumerable<Entity> contextLocalGoalTypes)
        {
            foreach (dynamic localOption in contextLocalGoalTypes)
            {
                localOption.IsActive = true;
            }

        }
    }
}
