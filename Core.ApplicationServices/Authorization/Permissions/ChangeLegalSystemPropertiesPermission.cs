namespace Core.ApplicationServices.Authorization.Permissions
{
    public class ChangeLegalSystemPropertiesPermission : Permission
    {
        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
