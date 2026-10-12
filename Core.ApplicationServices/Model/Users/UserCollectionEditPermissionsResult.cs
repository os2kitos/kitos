using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using System.Linq;

namespace Core.ApplicationServices.Model.Users
{
    public class UserCollectionEditPermissionsResult
    {
        public UserCollectionEditPermissionsResult(bool editProperties, bool editContractRole, bool editSystemRole, bool editOrganizationRole, bool editLocalAdminRole)
        {
            EditProperties = editProperties;
            EditContractRole = editContractRole;
            EditSystemRole = editSystemRole;
            EditOrganizationRole = editOrganizationRole;
            EditLocalAdminRole = editLocalAdminRole;

            CanEditAny = editProperties || editContractRole || editSystemRole || editOrganizationRole;
        }

        public bool CanEditAny { get; }
        public bool EditProperties { get; }
        public bool EditContractRole { get; }
        public bool EditSystemRole { get; }
        public bool EditLocalAdminRole { get; }
        public bool EditOrganizationRole { get; }

        public static UserCollectionEditPermissionsResult FromOrganization(
            Organization organization,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext organizationalUserContext)
        {
            var modify = authorizationContext.AllowModify(organization);

            if (modify && (organizationalUserContext.IsGlobalAdmin() || organizationalUserContext.HasRole(organization.Id, OrganizationRole.LocalAdmin)))
            {
                return new UserCollectionEditPermissionsResult(true, true, true, true, true);
            }

            var hasContractRole = organizationalUserContext.HasRole(organization.Id, OrganizationRole.ContractModuleAdmin);
            var hasSystemRole = organizationalUserContext.HasRole(organization.Id, OrganizationRole.SystemModuleAdmin);
            var hasOrganizationRole = organizationalUserContext.HasRole(organization.Id, OrganizationRole.OrganizationModuleAdmin);

            return new UserCollectionEditPermissionsResult(modify, hasContractRole, hasSystemRole, hasOrganizationRole, false);
        }
    }
}
