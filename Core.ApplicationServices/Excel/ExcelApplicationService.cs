using Core.ApplicationServices.Authorization;
using System;
using System.Web;
using Core.DomainServices.Generic;
using System.IO;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel.Organization;
using Core.ApplicationServices.Model.Excel;

namespace Core.ApplicationServices.Excel
{
    public class ExcelApplicationService : IExcelApplicationService
    {
        private readonly IExcelService _excelService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly string _mapPath = HttpContext.Current.Server.MapPath("~/Content/excel/");

        public ExcelApplicationService(IExcelService excelService,
            IAuthorizationContext authorizationContext,
            IEntityIdentityResolver entityIdentityResolver)
        {
            _excelService = excelService;
            _authorizationContext = authorizationContext;
            _entityIdentityResolver = entityIdentityResolver;
        }

        public Result<ExcelExportModel, OperationError> GetUsers(Guid organizationUuid, bool? exportUsers)
        {
            return GetExcelFile(organizationUuid, _excelService.ExportUsers);
        }

        public Result<ExcelExportModel, OperationError> GetOrgUnits(Guid organizationUuid, bool? exportOrgUnits)
        {
            return GetExcelFile(organizationUuid, _excelService.ExportOrganizationUnits);
        }

        public Result<ExcelExportModel, OperationError> GetContracts(Guid organizationUuid, bool? exportContracts)
        {
            return GetExcelFile(organizationUuid, _excelService.ExportItContracts);
        }

        public Result<int, OperationError> ResolveOrganizationIdAndValidateAccess(Guid organizationUuid)
        {
            return ResolveOrganizationId(organizationUuid)
                .Bind(organizationId => AllowAccess(organizationId)
                    ? Result<int, OperationError>.Success(organizationId)
                    : new OperationError("User is not allowed to perform batch import", OperationFailure.Forbidden));
        }

        private delegate Stream ExportDelegate(Stream stream, int organizationId);
        private Result<ExcelExportModel, OperationError> GetExcelFile(Guid organizationUuid,
            ExportDelegate exportMethod)
        {
            var organizationIdResult = ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (organizationIdResult.Failed)
                return organizationIdResult.Error;

            const string filename = "OS2KITOS Organisationsenheder.xlsx";
            var stream = new MemoryStream();
            using (var file = File.OpenRead(_mapPath + filename))
                file.CopyTo(stream);

            exportMethod(stream, organizationIdResult.Value);
            stream.Seek(0, SeekOrigin.Begin);
            return new ExcelExportModel(stream, filename);
        }

        private Result<int, OperationError> ResolveOrganizationId(Guid organizationUuid)
        {
            var organizationId = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (organizationId.IsNone)
            {
                return new OperationError($"Organization with uuid: '{organizationUuid}' was not found",
                    OperationFailure.NotFound);
            }

            return organizationId.Value;
        }

        private bool AllowAccess(int organizationId)
        {
            return _authorizationContext.HasPermission(new BatchImportPermission(organizationId));
        }
    }
}
