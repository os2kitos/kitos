using System;
using System.Data.Entity;
using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Tasks
{
    public class DropDatabaseTask : IDatabaseTask
    {
        private readonly string _connectionString;

        public DropDatabaseTask(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public bool Execute()
        {
            System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseAlways<KitosContext>());

            using (var context = new KitosContext(_connectionString))
            {
                context.Database.Initialize(true);
            }

            return true;
        }
    }
}
