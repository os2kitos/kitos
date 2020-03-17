namespace Core.ApplicationServices.Authorization.Permissions
{
    public class TriggerBrokenReferencesReportPermission : Permission
    {
        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
