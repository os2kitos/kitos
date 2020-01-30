using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class AdministerOrganizationRightPermission : Permission
    {
        public OrganizationRight Target { get; }

        public AdministerOrganizationRightPermission(OrganizationRight target)
        {
            Target = target;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
