using Core.ApplicationServices.Authorization.Permissions;

namespace Core.ApplicationServices.Authorization
{
    public interface IPermissionVisitor
    {
        bool Visit(BatchImportPermission permission);
        bool Visit(SystemUsageMigrationPermission permission);
        bool Visit(VisibilityControlPermission permission);
        bool Visit(AdministerOrganizationRightPermission permission);
        bool Visit(DefineOrganizationTypePermission permission);
        bool Visit(CreateEntityWithVisibilityPermission permission);
        bool Visit(ViewBrokenExternalReferencesReportPermission permission);
        bool Visit(TriggerBrokenReferencesReportPermission permission);
        bool Visit(AdministerGlobalPermission permission);
        bool Visit(ImportHierarchyFromStsOrganizationPermission permission);
        bool Visit(BulkAdministerOrganizationUnitRegistrations permission);
        bool Visit(DeleteAnyUserPermission permission);

        bool Visit(ChangeLegalSystemPropertiesPermission permission);
    }
}
