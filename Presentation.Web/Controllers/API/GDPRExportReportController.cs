using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.ItSystemUsage.GDPR;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/gdpr-report")]
    public class GdprExportReportController : BaseApiController
    {
        private readonly IGDPRExportService _gdprExportService;

        public GdprExportReportController(IGDPRExportService gdprExportService)
        {
            _gdprExportService = gdprExportService;
        }

        [HttpGet]
        [Route("csv/{orgId}")]
        public HttpResponseMessage GetCurrentCsvReport(int orgId)
        {
            return _gdprExportService
                .GetGDPRData(orgId)
                .Match
                (
                    onSuccess: GDPRExportReportCsvMapper.CreateReportCsvResponse,
                    onFailure: FromOperationError
                );
        }
    }
}