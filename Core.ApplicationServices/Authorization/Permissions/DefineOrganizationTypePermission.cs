using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class DefineOrganizationTypePermission : Permission
    {
        public OrganizationTypeKeys TargetOrganizationType { get; }

        public DefineOrganizationTypePermission(OrganizationTypeKeys targetOrganizationType)
        {
            TargetOrganizationType = targetOrganizationType;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
