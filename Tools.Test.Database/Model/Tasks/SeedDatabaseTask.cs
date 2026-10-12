using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Tasks
{
    public class SeedDatabaseTask : DatabaseTask
    {
        public override bool Execute(KitosContext context)
        {
            KitosContextSeeder.Seed(context);
            return true;
        }
    }
}
