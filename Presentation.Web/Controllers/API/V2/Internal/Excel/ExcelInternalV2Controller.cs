using System;
using Core.ApplicationServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using Presentation.Web.Models.API.V2.Internal.Response.Excel;
using Swashbuckle.Swagger.Annotations;
using Core.ApplicationServices.Model.Excel;
using Core.Abstractions.Types;
using Presentation.Web.Helpers;

namespace Presentation.Web.Controllers.API.V2.Internal.Excel
{
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/local-admin/excel")]
    public class ExcelInternalV2Controller : InternalApiV2Controller
    {
        private readonly IExcelService _excelService;
        private readonly string _mapPath = HttpContext.Current.Server.MapPath(Constants.Excel.ExcelFilePath);

        public ExcelInternalV2Controller(IExcelService excelService)
        {
            _excelService = excelService;
        }

        #region Excel Users

        [HttpGet]
        [Route("users")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetUsers(Guid organizationUuid, bool? exportUsers)
        {
            return GetExcelFile(organizationUuid, Constants.Excel.UserFileName, _excelService.ExportUsers)
                .Match(result => GetResponseMessage(result.MemoryStream, result.FileName), FromOperationError);
        }

        [HttpPost]
        [Route("users")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<IHttpActionResult> PostUsers(Guid organizationUuid, bool? importUsers)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);

            var organizationId = result.Value;

            ValidateRequestContainsMultipartFormData();

            // read multipart form data
            using var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportUsers(stream, organizationId);
                return Ok();
            }
            catch (ExcelImportException e)
            {
                return Content(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel OrganizationUnits

        [HttpGet]
        [Route("units")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetOrgUnits(Guid organizationUuid, bool? exportOrgUnits)
        {
            return GetExcelFile(organizationUuid, Constants.Excel.UnitFileName, _excelService.ExportOrganizationUnits)
                .Match(result => GetResponseMessage(result.MemoryStream, result.FileName), FromOperationError);
        }

        [HttpPost]
        [Route("units")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<IHttpActionResult> PostOrgUnits(Guid organizationUuid, bool? importOrgUnits)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);

            var organizationId = result.Value;

            ValidateRequestContainsMultipartFormData();

            // read multipart form data
            using var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportOrganizationUnits(stream, organizationId);
                return Ok();
            }
            catch (ExcelImportException e)
            {
                return Content(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel IT Contracts

        [HttpGet]
        [Route("contracts")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetContracts(Guid organizationUuid, bool? exportContracts)
        {
            return GetExcelFile(organizationUuid, Constants.Excel.ContractsFileName, _excelService.ExportItContracts)
                .Match(result => GetResponseMessage(result.MemoryStream, result.FileName), FromOperationError);
        }

        [HttpPost]
        [Route("contracts")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<IHttpActionResult> PostContracts(Guid organizationUuid, bool? importContracts)
        {
            var result = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);

            var organizationId = result.Value;

            ValidateRequestContainsMultipartFormData();

            // read multipart form data
            using var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportItContracts(stream, organizationId);
                return Ok();
            }
            catch (ExcelImportException e)
            {
                return Content(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Helpers

        private delegate Stream ExportDelegate(Stream stream, int organizationId);
        private Result<ExcelExportModel, OperationError> GetExcelFile(Guid organizationUuid, string fileName,
            ExportDelegate exportMethod)
        {
            var organizationIdResult = _excelService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (organizationIdResult.Failed)
                return organizationIdResult.Error;

            var stream = new MemoryStream();
            using (var file = File.OpenRead(_mapPath + fileName))
                file.CopyTo(stream);

            exportMethod(stream, organizationIdResult.Value);
            stream.Seek(0, SeekOrigin.Begin);
            return new ExcelExportModel(stream, fileName);
        }

        private static IHttpActionResult GetResponseMessage(Stream stream, string filename)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var result = new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) });
            var mimeType = MimeMapping.GetMimeMapping(filename);
            result.Response.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            result.Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = filename
            };
            return result;
        }

        private static IEnumerable<ExcelImportErrorV2DTO> GetErrorMessages(ExcelImportException e)
        {
            return e.Errors.Select(x => new ExcelImportErrorV2DTO
            {
                Column = x.Column,
                Message = x.Message,
                Row = x.Row,
                SheetName = x.SheetName
            }).ToList();
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

        private void ValidateRequestContainsMultipartFormData()
        {
            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        #endregion
    }
}