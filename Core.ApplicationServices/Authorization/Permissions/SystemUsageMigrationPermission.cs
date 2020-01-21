namespace Core.ApplicationServices.Authorization.Permissions
{
    public class SystemUsageMigrationPermission : Permission
    {
        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
