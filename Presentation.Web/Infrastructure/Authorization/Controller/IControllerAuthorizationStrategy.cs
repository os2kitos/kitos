using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public interface IControllerAuthorizationStrategy
    {
        bool ApplyBaseQueryPostProcessing { get; }
        bool AllowOrganizationReadAccess(int organizationId);
        bool AllowRead(IEntity entity);
        bool AllowCreate<T>(IEntity entity);
        bool AllowModify(IEntity entity);
        bool AllowDelete(IEntity entity);
        bool AllowEntityVisibilityControl(IEntity entity);
    }
}
