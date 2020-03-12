using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.DomainModel;
using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Tasks
{
    public class EnableAllOptionsTask : DatabaseTask
    {
        public override bool Execute(KitosContext context)
        {
            var optionTypes = LoadAllOptionTypes();

            foreach (var optionType in optionTypes)
            {
                Console.Out.WriteLine("Enabling all options of type:" + optionType.Name);
                var dbSet = context.Set(optionType).ToListAsync().GetAwaiter().GetResult();
                EnableAllLocalOptions(dbSet);
                context.SaveChanges();
            }

            return true;
        }

        private static IEnumerable<Type> LoadAllOptionTypes()
        {
            var optionTypes =
                typeof(OptionEntity<>)
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
                && baseType.GetGenericTypeDefinition() == typeof(OptionEntity<>);
        }

        private static void EnableAllLocalOptions(IEnumerable<object> contextLocalGoalTypes)
        {
            foreach (dynamic localOption in contextLocalGoalTypes)
            {
                localOption.IsLocallyAvailable = true;
                localOption.IsEnabled = true;
                localOption.IsObligatory = true;    //Makes it immediately available in all organizations
            }
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}.";
        }
    }
}
