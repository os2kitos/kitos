using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Presentation.Web.Infrastructure.Authorization.Controller.General
{
    public class ContextBasedAuthorizationStrategy : IControllerAuthorizationStrategy
    {
        private readonly IAuthorizationContext _authorizationContext;

        public ContextBasedAuthorizationStrategy(IAuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
        }

        public CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess()
        {
            return _authorizationContext.GetCrossOrganizationReadAccess();
        }

        public OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return _authorizationContext.GetOrganizationReadAccessLevel(organizationId);
        }

        public bool AllowRead(IEntity entity)
        {
            return _authorizationContext.AllowReads(entity);
        }

        public bool AllowCreate<T>(IEntity entity)
        {
            //Entity instance is not used going forward
            return _authorizationContext.AllowCreate<T>(entity);
        }

        public bool AllowCreate<T>()
        {
            return _authorizationContext.AllowCreate<T>();
        }

        public bool AllowModify(IEntity entity)
        {
            return _authorizationContext.AllowModify(entity);
        }

        public bool AllowDelete(IEntity entity)
        {
            return _authorizationContext.AllowDelete(entity);
        }

        public bool AllowEntityVisibilityControl(IEntity entity)
        {
            return _authorizationContext.AllowEntityVisibilityControl(entity);
        }
    }
}