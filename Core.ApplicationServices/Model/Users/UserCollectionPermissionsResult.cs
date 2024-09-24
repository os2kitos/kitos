using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users
{
    public class UserCollectionPermissionsResult
    {
        public UserCollectionPermissionsResult(bool create, bool edit, bool delete)
        {
            Create = create;
            Edit = edit;
            Delete = delete;
        }

        public bool Create { get; }
        public bool Edit { get; }
        public bool Delete { get; }

        public static UserCollectionPermissionsResult FromOrganization(
            Organization organization,
            IAuthorizationContext authorizationContext)
        {
            var create = authorizationContext.AllowCreate<User>(organization.Id);
            var modify = authorizationContext.AllowModify(organization);
            var delete = authorizationContext.HasPermission(new DeleteAnyUserPermission(organization.Id));
            return new UserCollectionPermissionsResult(create, modify, delete);
        }
    }
}
