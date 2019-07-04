using System.Data.Entity;
using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Tasks
{
    public class DropDatabaseTask : DatabaseTask
    {
        private readonly string _connectionString;

        public DropDatabaseTask(string connectionString)
        : base(connectionString)
        {
            _connectionString = connectionString;
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
