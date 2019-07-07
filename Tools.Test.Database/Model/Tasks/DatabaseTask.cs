using System;
using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Tasks
{
    public abstract class DatabaseTask
    {
        private readonly string _connectionString;

        protected DatabaseTask(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public abstract bool Execute();

        protected KitosContext CreateKitosContext()
        {
            return new KitosContext(_connectionString);
        }
    }
}
