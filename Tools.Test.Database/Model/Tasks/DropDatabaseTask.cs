using System;
using System.Data.Entity;
using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Tasks
{
    public class DropDatabaseTask : DatabaseTask
    {
        public DropDatabaseTask(string connectionString)
        : base(connectionString)
        {

        }

        public override bool Execute()
        {
            System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseAlways<KitosContext>());

            using (var context = CreateKitosContext())
            {
                context.Database.Initialize(true);
            }

            return true;
        }
    }
}
