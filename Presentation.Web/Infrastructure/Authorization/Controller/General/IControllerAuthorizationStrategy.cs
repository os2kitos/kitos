using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Presentation.Web.Infrastructure.Authorization.Controller.General
{
    public interface IControllerAuthorizationStrategy
    {
        CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess();
        OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId);
        bool AllowRead(IEntity entity);
        bool AllowCreate<T>(IEntity entity);
        bool AllowCreate<T>();
        bool AllowModify(IEntity entity);
        bool AllowDelete(IEntity entity);
        bool HasPermission(Permission permission);
    }
}
