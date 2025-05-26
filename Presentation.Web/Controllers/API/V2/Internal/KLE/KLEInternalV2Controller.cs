using Core.ApplicationServices.KLE;
using Core.DomainModel.KLE;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices;
using Core.ApplicationServices.Extensions;
using Presentation.Web.Controllers.API.V2.Internal.KLE.Mapping;
using Presentation.Web.Helpers;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Models.API.V2.Internal.Response.KLE;

namespace Presentation.Web.Controllers.API.V2.Internal.KLE
{
    [RoutePrefix("api/v2/internal/kle")]
    public class KLEInternalV2Controller : InternalApiV2Controller
    {
        private const string KLETypeColumnName = Constants.KLE.Type.Column;
        private const string KLETaskKeyColumnName = Constants.KLE.TaskKey.Column;
        private const string KLEDescriptionColumnName = Constants.KLE.Description.Column;
        private const string KLEChangeColumnName = Constants.KLE.Change.Column;
        private const string KLEChangeDetailsColumnName = Constants.KLE.ChangeDetails.Column;

        private readonly IKLEApplicationService _kleApplicationService;

        public KLEInternalV2Controller(IKLEApplicationService kleApplicationService)
        {
            _kleApplicationService = kleApplicationService;
        }

        [HttpGet]
        [Route("status")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(KLEStatusResponseDTO))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult GetKLEStatus()
        {
            var result = _kleApplicationService.GetKLEStatus();

            return result.Ok ?
                Ok(
                    new KLEStatusResponseDTO
                    {
                        UpToDate = result.Value.UpToDate,
                        Version = result.Value.Published.ConvertToDanishFormatDateString()
                    }) :
                FromOperationFailure(result.Error);
        }

        [HttpGet]
        [Route("changes")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult GetKLEChanges()
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
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult PutKLEChanges()
        {
            var result = _kleApplicationService.UpdateKLE();

            return result.Ok ?
                Ok(
                    new KLEUpdateResponseDTO
                    {
                        Status = result.Value.ToChoice()
                    }) :
                FromOperationFailure(result.Error);
        }

        #region Helpers

        private static void CreateCsvHeader(ICollection<dynamic> list)
        {
            var header = new ExpandoObject() as IDictionary<string, object>;
            header.Add(KLETypeColumnName, Constants.KLE.Type.ColumnName);
            header.Add(KLETaskKeyColumnName, Constants.KLE.TaskKey.ColumnName);
            header.Add(KLEDescriptionColumnName, Constants.KLE.Description.ColumnName);
            header.Add(KLEChangeColumnName, Constants.KLE.Change.ColumnName);
            header.Add(KLEChangeDetailsColumnName, Constants.KLE.ChangeDetails.ColumnName);
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
            }
        }

        private static IHttpActionResult CreateCsvFormattedHttpResponse(IEnumerable<dynamic> list)
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
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.KLE.MediaTypeHeaderValue);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.KLE.DispositionType)
            {
                FileNameStar = Constants.KLE.FileNameStar,
                DispositionType = Constants.KLE.DispositionType

            };
            return new ResponseMessageResult(result);
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

        #endregion
    }
}