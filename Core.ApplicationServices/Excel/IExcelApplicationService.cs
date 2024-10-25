using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Excel;
using System;

namespace Core.ApplicationServices.Excel
{
    public interface IExcelApplicationService
    {
        Result<ExcelExportModel, OperationError> GetUsers(Guid organizationUuid, bool? exportUsers);
        Result<ExcelExportModel, OperationError> GetOrgUnits(Guid organizationUuid, bool? exportOrgUnits);
        Result<ExcelExportModel, OperationError> GetContracts(Guid organizationUuid, bool? exportContracts);

        Result<int, OperationError> ResolveOrganizationIdAndValidateAccess(Guid organizationUuid);
    }
}
