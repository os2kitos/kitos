using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Qa;
using Core.DomainModel.Qa.References;
using Core.DomainModel.Result;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.Qa;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/broken-external-references-report")]
    public class BrokenExternalReferencesReportController : BaseApiController
    {
        private readonly IBrokenExternalReferencesReportService _brokenExternalReferencesReportService;

        public BrokenExternalReferencesReportController(IBrokenExternalReferencesReportService brokenExternalReferencesReportService)
        {
            _brokenExternalReferencesReportService = brokenExternalReferencesReportService;
        }

        [HttpGet]
        [Route("status")]
        public HttpResponseMessage GetStatus()
        {
            return _brokenExternalReferencesReportService
                .GetLatestReport()
                .Match
                (
                    onSuccess: report => Ok(MapStatus(report)),
                    onFailure: error => error.FailureType == OperationFailure.NotFound
                        ? Ok(MapStatus(Maybe<BrokenExternalReferencesReport>.None))
                        : FromOperationError(error)
                );
        }

        [HttpPost]
        [Route("trigger")]
        public HttpResponseMessage Trigger()
        {
            return _brokenExternalReferencesReportService
                .TriggerReportGeneration()
                .Match
                (
                    onValue: FromOperationError,
                    onNone: () => CreateResponse(HttpStatusCode.Accepted)
                );
        }

        [HttpGet]
        [Route("current/csv")]
        public HttpResponseMessage GetCurrentCsvReport()
        {
            return _brokenExternalReferencesReportService
                .GetLatestReport()
                .Match
                (
                    onSuccess: BrokenExternalReferencesReportCsvMapper.CreateReportCsvResponse,
                    onFailure: FromOperationError
                );
        }

        private static BrokenExternalReferencesReportStatusDTO MapStatus(Maybe<BrokenExternalReferencesReport> reportResult)
        {
            return reportResult
                .Match
                (
                    onValue: report => new BrokenExternalReferencesReportStatusDTO
                    {
                        Available = true,
                        CreatedDate = report.Created,
                        BrokenLinksCount = report.GetBrokenLinks().Count()
                    },
                    onNone: () => new BrokenExternalReferencesReportStatusDTO
                    {
                        Available = false
                    }
                );
        }
    }
}