namespace Core.ApplicationServices.Authorization.Permissions
{
    public class BulkAdministerOrganizationUnitRegistrations : Permission
    {
        public int OrganizationId { get; }

        public BulkAdministerOrganizationUnitRegistrations(int organizationId)
        {
            OrganizationId = organizationId;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
