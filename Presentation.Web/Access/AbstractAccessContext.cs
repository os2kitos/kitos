using Core.DomainModel.ItSystem;

namespace Presentation.Web.Access
{
    public abstract class AbstractAccessContext
    {
        public virtual bool AllowCreate(int userId)
        {
            return false;
        }

        public virtual bool AllowReads(int userId)
        {
            return false;
        }

        public virtual bool AllowReads(int userId, ItSystem entity)
        {
            return false;
        }

        public virtual bool AllowUpdates(int userId)
        {
            return false;
        }
    }
}