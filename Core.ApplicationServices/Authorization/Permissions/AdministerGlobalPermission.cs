namespace Core.ApplicationServices.Authorization.Permissions
{
    public class AdministerGlobalPermission : Permission
    {
        public GlobalPermission Permission { get; }

        public AdministerGlobalPermission(GlobalPermission permission)
        {
            Permission = permission;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
