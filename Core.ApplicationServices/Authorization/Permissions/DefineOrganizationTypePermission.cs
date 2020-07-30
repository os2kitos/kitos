using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class DefineOrganizationTypePermission : Permission
    {
        public OrganizationTypeKeys TargetOrganizationType { get; }
        public int ParentOrganizationId { get; }

        public DefineOrganizationTypePermission(OrganizationTypeKeys targetOrganizationType, int parentOrganizationId)
        {
            TargetOrganizationType = targetOrganizationType;
            ParentOrganizationId = parentOrganizationId;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
