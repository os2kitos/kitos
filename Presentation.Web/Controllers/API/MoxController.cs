using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.API
{
    public class MoxController : BaseApiController
    {
        private readonly IMoxService _moxService;

        public MoxController(IMoxService moxService)
        {
            _moxService = moxService;
        }

        public HttpResponseMessage Get(int organizationId)
        {
            using (var stream = new MemoryStream())
            {
                _moxService.Import(stream, organizationId, KitosUser);
                stream.Seek(0, SeekOrigin.Begin);
                var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "OS2KITOS MOX Skabelon Organisation.xlsx"
                };
                return result;
            }            
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

                var file = provider.Contents.First();
                var filename = Path.GetFileName(file.Headers.ContentDisposition.FileName);
                var buffer = await file.ReadAsByteArrayAsync();
                var stream = new MemoryStream(buffer);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
