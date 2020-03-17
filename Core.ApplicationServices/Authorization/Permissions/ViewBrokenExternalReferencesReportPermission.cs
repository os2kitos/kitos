using Core.DomainModel.Qa.References;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class ViewBrokenExternalReferencesReportPermission : Permission
    {
        public BrokenExternalReferencesReport Target { get; }

        public ViewBrokenExternalReferencesReportPermission(BrokenExternalReferencesReport target)
        {
            Target = target;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
