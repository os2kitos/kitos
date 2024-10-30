using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/local-admin/excel")]
    public class ExcelController : BaseApiController
    {
        private readonly IExcelService _excelService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly string _mapPath = HttpContext.Current.Server.MapPath(Constants.Excel.ExcelFilePath);

        public ExcelController(IExcelService excelService, IAuthorizationContext authorizationContext) 
        {
            _excelService = excelService;
            _authorizationContext = authorizationContext;
        }

        #region Excel Users

        [HttpGet]
        [Route("users-by-id")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUsers(int organizationId, bool? exportUsers)
        {
            if (!AllowAccess(organizationId))
            {
                return Forbidden();
            }

            return GetUsersExcel(organizationId);
        }

        [HttpGet]
        [Route("users-by-uuid")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUsersByUuid(Guid organizationUuid, bool? exportUsers)
        {
            var organizationIdResult = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (organizationIdResult.Failed)
                return FromOperationError(organizationIdResult.Error.FailureType);

            return GetUsersExcel(organizationIdResult.Value);
        }

        private HttpResponseMessage GetUsersExcel(int organizationId)
        {
            return GetExcelFile(organizationId, Constants.Excel.UserFileName, _excelService.ExportUsers);
        }

        [HttpPost]
        [Route("users-by-id")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> PostUsers(int organizationId, bool? importUsers)
        {
            if (!AllowAccess(organizationId))
            {
                return Forbidden();
            }

            return await PostUsersExcel(organizationId, importUsers);
        }

        [HttpPost]
        [Route("users-by-uuid")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> PostUsersByUuid(Guid organizationUuid, bool? importUsers)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);

            var organizationId = result.Value;

            return await PostUsersExcel(organizationId, importUsers);
        }

        private async Task<HttpResponseMessage> PostUsersExcel(int organizationId, bool? importUsers)
        {
            ValidateRequestContainsMultipartFormData();

            // read multipart form data
            using var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportUsers(stream, organizationId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel OrganizationUnits

        [HttpGet]
        [Route("units-by-id")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetOrgUnits(int organizationId, bool? exportOrgUnits)
        {
            return GetOrgUnitsExcel(organizationId);
        }

        [HttpGet]
        [Route("units-by-uuid")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetOrgUnitsByUuid(Guid organizationUuid, bool? exportOrgUnits)
        {
            var organizationIdResult = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (organizationIdResult.Failed)
                return FromOperationError(organizationIdResult.Error.FailureType);

            return GetOrgUnitsExcel(organizationIdResult.Value);
        }

        private HttpResponseMessage GetOrgUnitsExcel(int organizationId)
        {
            return GetExcelFile(organizationId, Constants.Excel.UnitFileName, _excelService.ExportOrganizationUnits);
        }

        [HttpPost]
        [Route("units-by-id")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> PostOrgUnits(int organizationId, bool? importOrgUnits)
        {
            if (!AllowAccess(organizationId))
            {
                return Forbidden();
            }
            return await PostOrgUnitsExcel(organizationId, importOrgUnits);
        }

        [HttpPost]
        [Route("units-by-uuid")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> PostOrgUnitsByUuid(Guid organizationUuid, bool? importOrgUnits)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);
            var organizationId = result.Value;

            return await PostOrgUnitsExcel(organizationId, importOrgUnits);
        }

        private async Task<HttpResponseMessage> PostOrgUnitsExcel(int organizationId, bool? importOrgUnits)
        {
            
            ValidateRequestContainsMultipartFormData();

            // read multipart form data
            using var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportOrganizationUnits(stream, organizationId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel IT Contracts

        [HttpGet]
        [Route("contracts-by-id")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetContracts(int organizationId, bool? exportContracts)
        {
            if (!AllowAccess(organizationId))
            {
                return Forbidden();
            }

            return GetContractsExcel(organizationId);
        }

        [HttpGet]
        [Route("contracts-by-uuid")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetContractsByUuid(Guid organizationUuid, bool? exportContracts)
        {
            var organizationIdResult = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (organizationIdResult.Failed)
                return FromOperationError(organizationIdResult.Error.FailureType);

            return GetContractsExcel(organizationIdResult.Value);
        }

        private HttpResponseMessage GetContractsExcel(int organizationId)
        {
            return GetExcelFile(organizationId, Constants.Excel.ContractsFileName, _excelService.ExportOrganizationUnits);
        }

        [HttpPost]
        [Route("contracts-by-id")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> PostContracts(int organizationId, bool? importContracts)
        {
            if (!AllowAccess(organizationId))
            {
                return Forbidden();
            }

            return await PostContractsExcel(organizationId, importContracts);
        }

        [HttpPost]
        [Route("contracts-by-uuid")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> PostContractsByUuid(Guid organizationUuid, bool? importContracts)
        {
            var organizationIdResult = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (organizationIdResult.Failed)
                return FromOperationError(organizationIdResult.Error.FailureType);

            return await PostContractsExcel(organizationIdResult.Value, importContracts);
        }

        private async Task<HttpResponseMessage> PostContractsExcel(int organizationId, bool? importContracts)
        {
            ValidateRequestContainsMultipartFormData();

            // read multipart form data
            using var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportItContracts(stream, organizationId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Helpers

        private delegate Stream ExportDelegate(Stream stream, int organizationId);
        private HttpResponseMessage GetExcelFile(int organizationId, string fileName,
            ExportDelegate exportMethod)
        {
            var stream = new MemoryStream();
            using (var file = File.OpenRead(_mapPath + fileName))
                file.CopyTo(stream);

            exportMethod(stream, organizationId);
            stream.Seek(0, SeekOrigin.Begin);
            return GetResponseMessage(stream, fileName);
        }

        private static HttpResponseMessage GetResponseMessage(Stream stream, string filename)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            var mimeType = MimeMapping.GetMimeMapping(filename);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = filename
            };
            return result;
        }

        private IEnumerable<ExcelImportErrorDTO> GetErrorMessages(ExcelImportException e)
        {
            return Map<IEnumerable<ExcelImportError>, IEnumerable<ExcelImportErrorDTO>>(e.Errors);
        }

        private void ValidateRequestContainsMultipartFormData()
        {
            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        private async Task<MemoryStream> ReadMultipartRequestAsync()
        {
            var provider = new MultipartMemoryStreamProvider();
            // Read the form data.
            await Request.Content.ReadAsMultipartAsync(provider);

            var file = provider.Contents[0];
            var buffer = await file.ReadAsByteArrayAsync();
            var stream = new MemoryStream(buffer);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private bool AllowAccess(int organizationId)
        {
            return _authorizationContext.HasPermission(new BatchImportPermission(organizationId));
        }

        #endregion
    }
}
