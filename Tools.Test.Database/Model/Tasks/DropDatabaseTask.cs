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
            using (var context = CreateKitosContext())
            {
                context.Database.Delete();
            }

            return true;
        }
    }
}
