using Core.DomainModel;
using Presentation.Web.Infrastructure.Authorization.Context;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public class ContextBasedAuthorizationStrategy : IControllerAuthorizationStrategy
    {
        private readonly IAuthorizationContext _authorizationContext;

        public ContextBasedAuthorizationStrategy(IAuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
        }

        public bool ApplyBaseQueryPostProcessing { get; } = true;

        public bool AllowOrganizationReadAccess(int organizationId)
        {
            return _authorizationContext.AllowReadsWithinOrganization(organizationId);
        }

        public bool AllowRead(IEntity entity)
        {
            return _authorizationContext.AllowReads(entity);
        }

        public bool AllowCreate<T>(IEntity entity)
        {
            //Entity instance is not used going forward
            return _authorizationContext.AllowCreate<T>();
        }

        public bool AllowModify(IEntity entity)
        {
            return _authorizationContext.AllowUpdates(entity);
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