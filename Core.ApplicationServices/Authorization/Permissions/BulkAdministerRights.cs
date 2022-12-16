namespace Core.ApplicationServices.Authorization.Permissions
{
    public class BulkAdministerRights : Permission
    {
        public int OrganizationId { get; }

        public BulkAdministerRights(int organizationId)
        {
            OrganizationId = organizationId;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
