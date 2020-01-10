using System;
using System.Net.Http;
using System.Web;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class UploadFileController : BaseApiController
    {
        public HttpResponseMessage Post()
        {
            if (AuthorizationContext.AllowBatchLocalImport() == false)
            {
                return Forbidden();
            }

            var context = HttpContext.Current.Request;

            var file = context.Files[0];
            var savePath = HttpContext.Current.Server.MapPath("~/Content/excel/");

            var fileExtension = file.FileName.Substring(file.FileName.IndexOf("."));

            try
            {
                file.SaveAs(savePath + "Kontrakt_Indgåelse_Skabelon" + fileExtension);
            }
            catch (Exception e) {
                return CreateResponse(System.Net.HttpStatusCode.InternalServerError,e);
            }

            return Ok();
        }
    }
}