namespace Core.ApplicationServices.Authorization
{
    public abstract class Permission
    {
        public abstract bool Accept(IPermissionVisitor permissionVisitor);
    }
}
