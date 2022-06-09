using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class ImportHierarchyFromStsOrganizationPermission : Permission
    {
        public Organization Organization { get; }

        public ImportHierarchyFromStsOrganizationPermission(Organization organization)
        {
            Organization = organization;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
