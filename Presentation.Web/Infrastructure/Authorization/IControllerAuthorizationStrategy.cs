using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization
{
    public interface IControllerAuthorizationStrategy
    {
        bool ApplyBaseQueryPostProcessing { get; }
        bool AllowOrganizationAccess(int organizationId);
        bool AllowReadAccess(IEntity entity);
        bool AllowWriteAccess(IEntity entity);
        bool AllowEntityVisibilityControl(IEntity entity);
    }
}
