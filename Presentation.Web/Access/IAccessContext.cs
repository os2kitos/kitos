using Core.DomainModel;

namespace Presentation.Web.Access
{
    public interface IAccessContext
    {
        bool AllowReads(int organizationId);
        bool AllowReads(IEntity entity);
        bool AllowUpdates(IEntity entity);
    }
}
