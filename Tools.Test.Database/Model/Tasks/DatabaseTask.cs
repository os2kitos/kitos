using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Tasks
{
    public abstract class DatabaseTask
    {
        public abstract bool Execute(KitosContext dbContext);
    }
}
