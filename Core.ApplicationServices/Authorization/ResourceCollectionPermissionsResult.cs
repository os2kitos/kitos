using Core.DomainModel;

namespace Core.ApplicationServices.Authorization
{
    public class ResourceCollectionPermissionsResult
    {
        public bool Create{ get; }

        public ResourceCollectionPermissionsResult(bool create)
        {
            Create = create;
        }

        public static ResourceCollectionPermissionsResult FromOrganizationId<T>(
            int organizationId,
            IAuthorizationContext authorizationContext) where T : IEntity
        {
            return new ResourceCollectionPermissionsResult(authorizationContext.AllowCreate<T>(organizationId));
        }
    }
}
