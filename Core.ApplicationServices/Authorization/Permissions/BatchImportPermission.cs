namespace Core.ApplicationServices.Authorization.Permissions
{
    public class BatchImportPermission : Permission
    {
        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
