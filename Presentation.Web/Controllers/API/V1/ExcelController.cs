using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [Route("api/local-admin/excel")]
    public class ExcelController : BaseApiController
    {
        private readonly IExcelService _excelService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IWebHostEnvironment _env;

        public ExcelController(IExcelService excelService, IAuthorizationContext authorizationContext, IWebHostEnvironment env)
        {
            _excelService = excelService;
            _authorizationContext = authorizationContext;
            _env = env;
        }

        private string GetMapPath(string fileName)
        {
            var excelPath = Constants.Excel.ExcelFilePath
                .TrimStart('~')
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return Path.Combine(_env.ContentRootPath, excelPath, fileName);
        }

        #region Excel Users

        [HttpGet]
        [Route("users-by-id")]
        public IActionResult GetUsers(int organizationId)
        {
            if (!AllowAccess(organizationId))
                return Forbidden();
            return GetUsersExcelFile(organizationId);
        }

        [HttpGet]
        [Route("users-by-uuid")]
        public IActionResult GetUsersByUuid(Guid organizationUuid)
        {
            return _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid)
                .Match(GetUsersExcelFile, FromOperationError);
        }

        private IActionResult GetUsersExcelFile(int organizationId)
        {
            return GetExcelFile(organizationId, Constants.Excel.UserFileName, _excelService.ExportUsers);
        }

        [HttpPost]
        [Route("users-by-id")]
        public async Task<IActionResult> PostUsers(int organizationId)
        {
            if (!AllowAccess(organizationId))
                return Forbidden();
            return await PostUsersExcel(organizationId);
        }

        [HttpPost]
        [Route("users-by-uuid")]
        public async Task<IActionResult> PostUsersByUuid(Guid organizationUuid)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);
            return await PostUsersExcel(result.Value);
        }

        private async Task<IActionResult> PostUsersExcel(int organizationId)
        {
            using var stream = await ReadMultipartRequestAsync();
            try
            {
                _excelService.ImportUsers(stream, organizationId);
                return Ok();
            }
            catch (ExcelImportException e)
            {
                return Conflict(GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel OrganizationUnits

        [HttpGet]
        [Route("units-by-id")]
        public IActionResult GetOrgUnits(int organizationId) => GetOrgUnitsExcelFile(organizationId);

        [HttpGet]
        [Route("units-by-uuid")]
        public IActionResult GetOrgUnitsByUuid(Guid organizationUuid)
        {
            return _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid)
                .Match(GetOrgUnitsExcelFile, FromOperationError);
        }

        private IActionResult GetOrgUnitsExcelFile(int organizationId)
        {
            return GetExcelFile(organizationId, Constants.Excel.UnitFileName, _excelService.ExportOrganizationUnits);
        }

        [HttpPost]
        [Route("units-by-id")]
        public async Task<IActionResult> PostOrgUnits(int organizationId)
        {
            if (!AllowAccess(organizationId))
                return Forbidden();
            return await PostOrgUnitsExcel(organizationId);
        }

        [HttpPost]
        [Route("units-by-uuid")]
        public async Task<IActionResult> PostOrgUnitsByUuid(Guid organizationUuid)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);
            return await PostOrgUnitsExcel(result.Value);
        }

        private async Task<IActionResult> PostOrgUnitsExcel(int organizationId)
        {
            using var stream = await ReadMultipartRequestAsync();
            try
            {
                _excelService.ImportOrganizationUnits(stream, organizationId);
                return Ok();
            }
            catch (ExcelImportException e)
            {
                return Conflict(GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel IT Contracts

        [HttpGet]
        [Route("contracts-by-id")]
        public IActionResult GetContracts(int organizationId)
        {
            if (!AllowAccess(organizationId))
                return Forbidden();
            return GetContractsExcelFile(organizationId);
        }

        [HttpGet]
        [Route("contracts-by-uuid")]
        public IActionResult GetContractsByUuid(Guid organizationUuid)
        {
            return _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid)
                .Match(GetContractsExcelFile, FromOperationError);
        }

        private IActionResult GetContractsExcelFile(int organizationId)
        {
            var fileName = Constants.Excel.ContractsFileName;
            var stream = new MemoryStream();
            using (var fileStream = System.IO.File.OpenRead(GetMapPath(fileName)))
                fileStream.CopyTo(stream);
            _excelService.ExportItContracts(stream, organizationId);
            return GetFileResult(stream, fileName);
        }

        [HttpPost]
        [Route("contracts-by-id")]
        public async Task<IActionResult> PostContracts(int organizationId)
        {
            if (!AllowAccess(organizationId))
                return Forbidden();
            return await PostContractsExcel(organizationId);
        }

        [HttpPost]
        [Route("contracts-by-uuid")]
        public async Task<IActionResult> PostContractsByUuid(Guid organizationUuid)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);
            return await PostContractsExcel(result.Value);
        }

        private async Task<IActionResult> PostContractsExcel(int organizationId)
        {
            using var stream = await ReadMultipartRequestAsync();
            try
            {
                _excelService.ImportItContracts(stream, organizationId);
                return Ok();
            }
            catch (ExcelImportException e)
            {
                return Conflict(GetErrorMessages(e));
            }
        }

        #endregion

        #region Helpers

        private delegate Stream ExportDelegate(Stream stream, int organizationId);

        private IActionResult GetExcelFile(int organizationId, string fileName, ExportDelegate exportMethod)
        {
            var stream = new MemoryStream();
            using (var fileStream = System.IO.File.OpenRead(GetMapPath(fileName)))
                fileStream.CopyTo(stream);
            exportMethod(stream, organizationId);
            return GetFileResult(stream, fileName);
        }

        private static IActionResult GetFileResult(Stream stream, string filename)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = filename
            };
        }

        private static IActionResult Conflict(IEnumerable<ExcelImportErrorDTO> errors)
        {
            return new ObjectResult(errors) { StatusCode = (int)HttpStatusCode.Conflict };
        }

        private IEnumerable<ExcelImportErrorDTO> GetErrorMessages(ExcelImportException e)
        {
            return Map<IEnumerable<ExcelImportError>, IEnumerable<ExcelImportErrorDTO>>(e.Errors);
        }

        private async Task<MemoryStream> ReadMultipartRequestAsync()
        {
            if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
                throw new InvalidOperationException("Expected multipart form data");

            var file = Request.Form.Files[0];
            var buffer = new byte[file.Length];
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var result = new MemoryStream(ms.ToArray());
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        private bool AllowAccess(int organizationId)
        {
            return _authorizationContext.HasPermission(new BatchImportPermission(organizationId));
        }

        #endregion
    }
}
