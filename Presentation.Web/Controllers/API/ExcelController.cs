using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Core.ApplicationServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ExcelController : BaseApiController
    {
        private readonly IExcelService _excelService;

        public ExcelController(IExcelService excelService)
        {
            _excelService = excelService;
        }

        public HttpResponseMessage Get(int organizationId)
        {
            var dir = HttpContext.Current.Server.MapPath("~/Content/excel/");
            var file = File.OpenRead(dir + "OS2KITOS Excel Skabelon Organisation.xlsx");
            var stream = new MemoryStream();
            
            file.CopyTo(stream);
            const string filename = "OS2KITOS Excel Skabelon Organisation.xlsx";
            _excelService.ExportOrganizationUnits(stream, organizationId, KitosUser);
            stream.Seek(0, SeekOrigin.Begin);
            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            var mimeType = MimeMapping.GetMimeMapping(filename);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = filename,
                DispositionType = "ISO-8859-1"
            };
            return result;
            
        }

        public HttpResponseMessage Get(int organizationId, bool? exportUsers)
        {
            var dir = HttpContext.Current.Server.MapPath("~/Content/excel/");
            var file = File.OpenRead(dir + "OS2KITOS Excel Skabelon Brugere.xlsx");
            var stream = new MemoryStream();
            
            file.CopyTo(stream);
            const string filename = "OS2KITOS Excel Skabelon Brugere.xlsx";
            _excelService.ExportUsers(stream, organizationId, KitosUser);
            stream.Seek(0, SeekOrigin.Begin);
            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            var mimeType = MimeMapping.GetMimeMapping(filename);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = filename,
                DispositionType = "ISO-8859-1"
            };
            return result;
            
        }

        public async Task<HttpResponseMessage> Post(int organizationId)
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                var file = provider.Contents[1];
                //var filename = Path.GetFileName(file.Headers.ContentDisposition.FileName);
                var buffer = await file.ReadAsByteArrayAsync();
                var stream = new MemoryStream(buffer);
                stream.Seek(0, SeekOrigin.Begin);
                _excelService.ImportOrganizationUnits(stream, organizationId, KitosUser);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                var errorsDto =
                    AutoMapper.Mapper.Map<IEnumerable<ExcelImportError>, IEnumerable<ExcelImportErrorDTO>>(e.Errors);
                return Request.CreateResponse(HttpStatusCode.Conflict, errorsDto);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        public async Task<HttpResponseMessage> Post(int organizationId, bool? importUsers)
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                var file = provider.Contents[1];
                //var filename = Path.GetFileName(file.Headers.ContentDisposition.FileName);
                var buffer = await file.ReadAsByteArrayAsync();
                var stream = new MemoryStream(buffer);
                stream.Seek(0, SeekOrigin.Begin);
                _excelService.ImportUsers(stream, organizationId, KitosUser);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ExcelImportException e)
            {
                var errorsDto =
                    AutoMapper.Mapper.Map<IEnumerable<ExcelImportError>, IEnumerable<ExcelImportErrorDTO>>(e.Errors);
                return Request.CreateResponse(HttpStatusCode.Conflict, errorsDto);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
