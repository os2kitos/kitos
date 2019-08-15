using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

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

        public virtual bool AllowUpdates(int userId, ItSystemUsage entity)
        {
            return false;
        }

        public virtual bool AllowDelete(int userId, ItSystemUsage entity)
        {
            return false;
        }
    }
}