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
using Core.ApplicationServices.TaskRefs;
using Core.DomainModel.KLE;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/kle")]
    public class KLEController : BaseApiController
    {
        private const string KLETypeColumnName = "Type";
        private const string KLETaskKeyColumnName = "TaskKey";
        private const string KLEDescriptionColumnName = "Description";
        private const string KLEChangeColumnName = "Change";
        private const string KLEChangeDetailsColumnName = "ChangeDescription";

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

            return result.Ok ?
                Ok(
                    new KLEStatusDTO
                    {
                        UpToDate = result.Value.UpToDate,
                        Version = result.Value.Published.ToString("dd-MM-yyyy")
                    }) :
                FromOperationFailure(result.Error);
        }

        [HttpGet]
        [Route("changes")]
        public HttpResponseMessage GetKLEChanges()
        {
            var result = _kleApplicationService.GetKLEChangeSummary();
            if (!result.Ok) return FromOperationFailure(result.Error);
            
            var list = new List<dynamic>();
            CreateCsvHeader(list);
            CreateCsvChangeDescriptions(list, result.Value);

            return CreateCsvFormattedHttpResponse(list);
        }

        [HttpPut]
        [Route("update")]
        public HttpResponseMessage PutKLEChanges()
        {
            var result = _kleApplicationService.UpdateKLE();
            
            return result.Ok ? 
                Ok(
                    new KLEUpdateDTO
                    {
                        Status = result.Value
                    }) :
                FromOperationFailure(result.Error);
        }

        #region Helpers

        private static void CreateCsvHeader(ICollection<dynamic> list)
        {
            var header = new ExpandoObject() as IDictionary<string, object>;
            header.Add(KLETypeColumnName, "KLE Type");
            header.Add(KLETaskKeyColumnName, "KLE nummer");
            header.Add(KLEDescriptionColumnName, "KLE beskrivelse");
            header.Add(KLEChangeColumnName, "Ændring");
            header.Add(KLEChangeDetailsColumnName, "Ændringsbeskrivelse");
            list.Add(header);
        }

        private void CreateCsvChangeDescriptions(ICollection<object> list, IEnumerable<KLEChange> kleChanges)
        {
            var relevantChanges = kleChanges.Where(c => c.ChangeType != KLEChangeType.UuidPatched);
            foreach (var elem in relevantChanges)
            {
                var obj = new ExpandoObject() as IDictionary<string, object>;
                obj.Add(KLETypeColumnName, elem.Type);
                obj.Add(KLETaskKeyColumnName, elem.TaskKey);
                obj.Add(KLEDescriptionColumnName, elem.UpdatedDescription);
                obj.Add(KLEChangeColumnName, ChangeTypeToString(elem.ChangeType));
                obj.Add(KLEChangeDetailsColumnName, elem.ChangeDetails);
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