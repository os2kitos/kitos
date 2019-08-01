using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Core.ApplicationServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ExcelController : BaseApiController
    {
        private readonly IExcelService _excelService;
        private readonly string _mapPath = HttpContext.Current.Server.MapPath("~/Content/excel/");

        public ExcelController(IExcelService excelService)
        {
            _excelService = excelService;
        }

        #region Excel Users

        public HttpResponseMessage GetUsers(int organizationId, bool? exportUsers)
        {
            const string filename = "OS2KITOS Brugere.xlsx";
            var stream = new MemoryStream();
            using (var file = File.OpenRead(_mapPath + filename))
                file.CopyTo(stream);

            _excelService.ExportUsers(stream, organizationId, KitosUser);
            stream.Seek(0, SeekOrigin.Begin);
            return GetResponseMessage(stream, filename);
        }

        public async Task<HttpResponseMessage> PostUsers(int organizationId, bool? importUsers)
        {
            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // read multipart form data
            var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportUsers(stream, organizationId, KitosUser);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel OrganizationUnits

        public HttpResponseMessage GetOrgUnits(int organizationId, bool? exportOrgUnits)
        {
            const string filename = "OS2KITOS Organisationsenheder.xlsx";
            var stream = new MemoryStream();
            using (var file = File.OpenRead(_mapPath + filename))
                file.CopyTo(stream);

            _excelService.ExportOrganizationUnits(stream, organizationId, KitosUser);
            stream.Seek(0, SeekOrigin.Begin);
            return GetResponseMessage(stream, filename);
        }

        public async Task<HttpResponseMessage> PostOrgUnits(int organizationId, bool? importOrgUnits)
        {
            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // read multipart form data
            var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportOrganizationUnits(stream, organizationId, KitosUser);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Excel IT Contracts

        public HttpResponseMessage GetContracts(int organizationId, bool? exportContracts)
        {
            const string filename = "OS2KITOS IT Kontrakter.xlsx";
            var stream = new MemoryStream();
            using (var file = File.OpenRead(_mapPath + filename))
                file.CopyTo(stream);

            _excelService.ExportItContracts(stream, organizationId, KitosUser);
            return GetResponseMessage(stream, filename);
        }

        public async Task<HttpResponseMessage> PostContracts(int organizationId, bool? importContracts)
        {
            // check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // read multipart form data
            var stream = await ReadMultipartRequestAsync();

            try
            {
                _excelService.ImportItContracts(stream, organizationId, KitosUser);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, GetErrorMessages(e));
            }
        }

        #endregion

        #region Helpers

        private static HttpResponseMessage GetResponseMessage(Stream stream, string filename)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream)};
            var mimeType = MimeMapping.GetMimeMapping(filename);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = filename
            };
            return result;
        }

        private static IEnumerable<ExcelImportErrorDTO> GetErrorMessages(ExcelImportException e)
        {
            return AutoMapper.Mapper.Map<IEnumerable<ExcelImportError>, IEnumerable<ExcelImportErrorDTO>>(e.Errors);
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
