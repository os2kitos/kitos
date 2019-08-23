using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization
{
    public class ContextBasedAuthorizationStrategy : IControllerAuthorizationStrategy
    {
        private readonly IAuthorizationContext _authorizationContext;

        public ContextBasedAuthorizationStrategy(IAuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
        }

        public bool ApplyBaseQueryPostProcessing { get; } = true;

        public bool AllowOrganizationAccess(int organizationId)
        {
            return _authorizationContext.AllowReadsWithinOrganization(organizationId);
        }

        public bool AllowReadAccess(IEntity entity)
        {
            return _authorizationContext.AllowReads(entity);
        }

        public bool AllowWriteAccess(IEntity entity)
        {
            return _authorizationContext.AllowUpdates(entity);
        }

        public bool AllowEntityVisibilityControl(IEntity entity)
        {
            return _authorizationContext.AllowEntityVisibilityControl(entity);
        }
    }
}