using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public interface IControllerAuthorizationStrategy
    {
        CrossOrganizationReadAccess GetCrossOrganizationReadAccess();
        /// <summary>
        /// Determines if the model allows for organizational refinement within the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool RequireGenericQueryPostFiltering<T>();
        bool AllowOrganizationReadAccess(int organizationId);
        bool AllowRead(IEntity entity);
        bool AllowCreate<T>(IEntity entity);
        bool AllowCreate<T>();
        bool AllowModify(IEntity entity);
        bool AllowDelete(IEntity entity);
        bool AllowEntityVisibilityControl(IEntity entity);
    }
}
