using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Qa;
using Core.DomainModel.Qa.References;
using Core.DomainModel.Result;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.Csv;
using Presentation.Web.Models.Qa;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/broken-external-references-report")]
    public class BrokenExternalReferencesReportController : BaseApiController
    {
        private const string UnknownValueString = "Ukendt";
        private static readonly string EmptyValueString = string.Empty;

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
                    onSuccess: CreateReportCsvResponse,
                    onFailure: FromOperationError
                );
        }

        private static HttpResponseMessage CreateReportCsvResponse(BrokenExternalReferencesReport report)
        {
            var csvResponseBuilder = new CsvResponseBuilder<IBrokenLink>();

            csvResponseBuilder =
                csvResponseBuilder
                    .WithFileName($"kitos_external_references_report-{report.Created:yyyy-MM-dd}.csv")
                    .WithColumn("Origin", "Oprindelse", MapBrokenLinkOriginType)
                    .WithColumn("OriginObjectName", "Navn", MapBrokenLinkOriginName)
                    .WithColumn("RefName", "Referencenavn", MapReferenceName)
                    .WithColumn("ErrorCategory", @"Fejlkategori", MapErrorCategory)
                    .WithColumn("ErrorCode", "Fejlkode", MapErrorCode)
                    .WithColumn("Url", "Url", link => link.ValueOfCheckedUrl);


            csvResponseBuilder = report
                .GetBrokenLinks()
                .Aggregate(csvResponseBuilder, (builder, brokenLink) => builder.WithRow(brokenLink));

            return csvResponseBuilder.Build();
        }

        private static string MapErrorCode(IBrokenLink arg)
        {
            return arg.ErrorResponseCode.HasValue ? arg.ErrorResponseCode.Value.ToString("D") : EmptyValueString;
        }

        private static string MapErrorCategory(IBrokenLink arg)
        {
            switch (arg.Cause)
            {
                case BrokenLinkCause.InvalidUrl:
                    return "Ugyldig URL";
                case BrokenLinkCause.DnsLookupFailed:
                    return "Ugyldigt domæne";
                case BrokenLinkCause.ErrorResponse:
                    return "Se fejlkode";
                case BrokenLinkCause.CommunicationError:
                    return "Kommunikationsfejl";
                default:
                    return UnknownValueString;
            }
        }

        private static string MapReferenceName(IBrokenLink arg)
        {
            switch (arg)
            {
                case BrokenLinkInExternalReference reference:
                    return reference.BrokenReferenceOrigin.Title;
                default:
                    return EmptyValueString;
            }
        }

        private static string MapBrokenLinkOriginName(IBrokenLink arg)
        {
            switch (arg)
            {
                case BrokenLinkInExternalReference reference:
                    return reference.BrokenReferenceOrigin.ItSystem.Name;
                case BrokenLinkInInterface linkInInterface:
                    return linkInInterface.BrokenReferenceOrigin.Name;
                default:
                    return UnknownValueString;
            }
        }

        private static string MapBrokenLinkOriginType(IBrokenLink arg)
        {
            switch (arg)
            {
                case BrokenLinkInExternalReference _:
                    return "IT System";
                case BrokenLinkInInterface _:
                    return "Snitflade";
                default:
                    return UnknownValueString;
            }
        }

        private static BrokenExternalReferencesReportStatusDTO MapStatus(Maybe<BrokenExternalReferencesReport> reportResult)
        {
            return reportResult
                .Match
                (
                    onValue: report => new BrokenExternalReferencesReportStatusDTO()
                    {
                        Available = true,
                        CreatedDate = report.Created,
                        BrokenLinksCount = report.GetBrokenLinks().Count()
                    },
                    onNone: () => new BrokenExternalReferencesReportStatusDTO()
                    {
                        Available = false
                    }
                );
        }
    }
}