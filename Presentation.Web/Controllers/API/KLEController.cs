using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]//TODO: Internalapi
    [RoutePrefix("api/v1/kle")]
    public class KLEController : BaseApiController
    {
        private readonly IKLEApplicationService _kleApplicationService;

        public KLEController(IAuthorizationContext authorizationContext, IKLEApplicationService kleApplicationService) : base(authorizationContext)
        {
            _kleApplicationService = kleApplicationService;
        }

        [HttpGet]
        [Route("status")]
        public HttpResponseMessage GetKLEStatus()
        {
            var result = _kleApplicationService.GetKLEStatus();

            switch (result.Status)
            {
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.Ok:
                    return Ok(
                        new KLEStatusDTO
                        {
                            UpToDate = result.Value.UpToDate,
                            Version = result.Value.Published.ToLongDateString()
                        });
                default:
                    return Error($"Something went wrong getting KLE status");
            }
        }

        [HttpGet]
        [Route("changes")]
        public HttpResponseMessage GetKLEChanges()
        {
            // TODO: Replace dummy data
            var list = new List<dynamic>();
            var header = new ExpandoObject() as IDictionary<string, Object>;
            header.Add("Uuid", "Identifier");
            header.Add("Type", "KLE Type");
            header.Add("TaskKey", "KLE nummer");
            header.Add("Description", "Beskrivelse");
            header.Add("Change", "Ændring");
            list.Add(header);
            var s = list.ToCsv();
            var bytes = Encoding.Unicode.GetBytes(s);
            var stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = "kle-updates.csv",
                DispositionType = "ISO-8859-1"
            };
            return result;
        }

        [HttpPut]
        [Route("")]
        public HttpResponseMessage PutKLEChanges()
        {
            // TODO
            return Ok();
        }
    }
}