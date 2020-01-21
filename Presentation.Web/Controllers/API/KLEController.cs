using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel.KLE;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/kle")]
    public class KLEController : BaseApiController
    {
        private readonly IKLEApplicationService _kleApplicationService;

        public KLEController(IKLEApplicationService kleApplicationService)
        {
            _kleApplicationService = kleApplicationService;
        }

        [HttpGet]
        [Route("status")]
        public HttpResponseMessage GetKLEStatus()
        {
            var result = _kleApplicationService.GetKLEStatus();

            switch (result.Ok)
            {
                case false:
                    return Forbidden();
                case true:
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

            switch (result.Ok)
            {
                case false:
                    return Forbidden();
                case true:
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

        [HttpPut]
        [Route("update")]
        public HttpResponseMessage PutKLEChanges()
        {
            var result = _kleApplicationService.UpdateKLE();
            switch (result.Ok)
            {
                case false:
                    return Forbidden();
                case true:
                {
                    return Ok(new KLEUpdateDTO
                    {
                        Status = result.Value
                    });
                }
                default:
                    return Error($"Something went wrong updating KLE values");
            }
        }

        #region Helpers

        private static void CreateCsvHeader(ICollection<dynamic> list)
        {
            var header = new ExpandoObject() as IDictionary<string, object>;
            header.Add("Type", "KLE Type");
            header.Add("TaskKey", "KLE nummer");
            header.Add("Description", "Beskrivelse");
            header.Add("Change", "Ændring");
            list.Add(header);
        }

        private void CreateCsvChangeDescriptions(ICollection<object> list, IEnumerable<KLEChange> kleChanges)
        {
            var relevantChanges = kleChanges.Where(c => c.ChangeType != KLEChangeType.UuidPatched);
            foreach (var elem in relevantChanges)
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null);
            }
        }

        #endregion
    }
}