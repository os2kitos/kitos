using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainServices.Repositories.KLE;
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
                            Version = result.Value.Published.ToString("dd-MM-yyyy")
                        });
                default:
                    return Error($"Something went wrong getting KLE status");
            }
        }

        [HttpGet]
        [Route("changes")]
        public HttpResponseMessage GetKLEChanges()
        {
            var result = _kleApplicationService.GetKLEChangeSummary();
            switch (result.Status)
            {
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.Ok:
                {
                    var list = new List<dynamic>();
                    CreateCsvHeader(list);
                    CreateCsvChangeDescriptions(list, result.Value);
                    return CreateCsvFormattedHttpResponse(list);
                }
                default:
                    return Error($"Something went wrong getting KLE status");
            }
        }

        private static void CreateCsvHeader(ICollection<dynamic> list)
        {
            var header = new ExpandoObject() as IDictionary<string, object>;
            //header.Add("Uuid", "Identifier");
            header.Add("Type", "KLE Type");
            header.Add("TaskKey", "KLE nummer");
            header.Add("Description", "Beskrivelse");
            header.Add("Change", "Ændring");
            list.Add(header);
        }

        private void CreateCsvChangeDescriptions(ICollection<object> list, IEnumerable<KLEChange> kleChanges)
        {
            foreach (var elem in kleChanges)
            {
                var obj = new ExpandoObject() as IDictionary<string, object>;
                obj.Add("Type", elem.Type);
                obj.Add("TaskKey", elem.TaskKey);
                obj.Add("Description", elem.UpdatedDescription);
                obj.Add("Change", ChangeTypeToString(elem.ChangeType));
                list.Add(obj);
            };
        }

        private static HttpResponseMessage CreateCsvFormattedHttpResponse(IEnumerable<dynamic> list)
        {
            var s = list.ToCsv();
            //var bytes = Encoding.Unicode.GetBytes(s);
            var bytes = Encoding.UTF8.GetBytes(s);
            var stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream),
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = "kle-updates.csv",
                DispositionType = "attachment"

            };
            return result;
        }

        private string ChangeTypeToString(KLEChangeType changeType)
        {
            switch (changeType)
            {
                case KLEChangeType.Removed: return "Fjernet";
                case KLEChangeType.Added: return "Tilføjet";
                case KLEChangeType.Renamed: return "Ændret";
                default:
                    throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null);
            }
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