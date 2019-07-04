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
            System.Data.Entity.Database.Delete(_connectionString);

            return true;
        }
    }
}
