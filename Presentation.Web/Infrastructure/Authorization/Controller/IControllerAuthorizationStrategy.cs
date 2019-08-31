using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public interface IControllerAuthorizationStrategy
    {
        //TODO: Add GetCrossOrganizationReadAccess : All | Public | None -> use that to determine queries
        bool ApplyBaseQueryPostProcessing { get; }
        bool AllowOrganizationReadAccess(int organizationId);
        bool AllowRead(IEntity entity);
        bool AllowCreate<T>(IEntity entity);
        bool AllowCreate<T>();
        bool AllowModify(IEntity entity);
        bool AllowDelete(IEntity entity);
        bool AllowEntityVisibilityControl(IEntity entity);
    }
}
