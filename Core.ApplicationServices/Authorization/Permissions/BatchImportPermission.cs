namespace Core.ApplicationServices.Authorization.Permissions
{
    public class BatchImportPermission : Permission
    {
        public int TargetOrganizationId { get; }

        public BatchImportPermission(int targetOrganizationId)
        {
            TargetOrganizationId = targetOrganizationId;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
