using Core.ApplicationServices.KLE;
using Core.DomainModel.KLE;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System;
using System.Linq;
using Core.ApplicationServices;
using Core.ApplicationServices.Extensions;
using Presentation.Web.Controllers.API.V2.Internal.KLE.Mapping;
using Presentation.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.KLE;

using System.Net;
namespace Presentation.Web.Controllers.API.V2.Internal.KLE
{
    [Route("api/v2/internal/kle")]
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
        [ApiResponse(typeof(KLEStatusResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        public IActionResult GetKLEStatus()
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
        [ApiResponse(HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        public IActionResult GetKLEChanges()
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
        [ApiResponse(HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        public IActionResult PutKLEChanges()
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

        private static FileStreamResult CreateCsvFormattedHttpResponse(IEnumerable<dynamic> list)
        {
            var s = list.ToCsv();
            var bytes = Encoding.UTF8.GetBytes(s);
            var stream = new MemoryStream(bytes);
            return new FileStreamResult(stream, Constants.KLE.MediaTypeHeaderValue)
            {
                FileDownloadName = Constants.KLE.FileNameStar
            };
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

