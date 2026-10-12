using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users
{
    public class UserCollectionPermissionsResult
    {
        public UserCollectionPermissionsResult(bool create, UserCollectionEditPermissionsResult edit, bool delete)
        {
            Create = create;
            Edit = edit;
            Delete = delete;
        }

        public bool Create { get; }
        public UserCollectionEditPermissionsResult Edit { get; }
        public bool Delete { get; }

        public static UserCollectionPermissionsResult FromOrganization(
            Organization organization,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext organizationalUserContext)
        {
            var create = authorizationContext.AllowCreate<User>(organization.Id);
            var delete = authorizationContext.HasPermission(new DeleteAnyUserPermission(organization.Id));

            return new UserCollectionPermissionsResult(create, UserCollectionEditPermissionsResult.FromOrganization(organization, authorizationContext, organizationalUserContext), delete);
        }
    }
}
