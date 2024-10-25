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
using Core.ApplicationServices.Excel;
using Presentation.Web.Models.API.V2.Internal.Response.Excel;

namespace Presentation.Web.Controllers.API.V2.Internal.Excel
{
    public class ExcelInternalV2Controller : InternalApiV2Controller
    {
        private readonly IExcelService _excelService;
        private readonly IExcelApplicationService _excelApplicationService;

        public ExcelInternalV2Controller(IExcelService excelService,
            IExcelApplicationService excelApplicationService)
        {
            _excelService = excelService;
            _excelApplicationService = excelApplicationService;
        }

        #region Excel Users

        [HttpGet]
        public IHttpActionResult GetUsers(Guid organizationUuid, bool? exportUsers)
        {
            return _excelApplicationService.GetUsers(organizationUuid, exportUsers)
                .Match(result => GetResponseMessage(result.MemoryStream, result.FileName), FromOperationError);
        }

        public async Task<IHttpActionResult> PostUsers(Guid organizationUuid, bool? importUsers)
        {
            var result = _excelApplicationService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);

            var organizationId = result.Value;
            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // read multipart form data
            var stream = await ReadMultipartRequestAsync();

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

        public IHttpActionResult GetOrgUnits(Guid organizationUuid, bool? exportOrgUnits)
        {
            return _excelApplicationService.GetUsers(organizationUuid, exportOrgUnits)
                .Match(result => GetResponseMessage(result.MemoryStream, result.FileName), FromOperationError);
        }

        public async Task<IHttpActionResult> PostOrgUnits(Guid organizationUuid, bool? importOrgUnits)
        {
            var result = _excelApplicationService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);

            var organizationId = result.Value;

            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // read multipart form data
            var stream = await ReadMultipartRequestAsync();

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

        public IHttpActionResult GetContracts(Guid organizationUuid, bool? exportContracts)
        {
            return _excelApplicationService.GetUsers(organizationUuid, exportContracts)
                .Match(result => GetResponseMessage(result.MemoryStream, result.FileName), FromOperationError);
        }

        public async Task<IHttpActionResult> PostContracts(Guid organizationUuid, bool? importContracts)
        {
            var result = _excelApplicationService.ResolveOrganizationIdAndValidateAccess(organizationUuid);
            if (result.Failed)
                return FromOperationError(result.Error);

            var organizationId = result.Value;

            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // read multipart form data
            var stream = await ReadMultipartRequestAsync();

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

        #endregion
    }
}