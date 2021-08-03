using System.Linq;
using System.Net.Http;
using Core.DomainModel.Qa.References;
using Presentation.Web.Models.Application.Csv;

namespace Presentation.Web.Models.API.V1.Qa
{
    public class BrokenExternalReferencesReportCsvMapper
    {
        private const string UnknownValueString = "Ukendt";
        private static readonly string EmptyValueString = string.Empty;

        public static HttpResponseMessage CreateReportCsvResponse(BrokenExternalReferencesReport report)
        {
            var csvResponseBuilder = new CsvResponseBuilder<IBrokenLink>();

            csvResponseBuilder =
                csvResponseBuilder
                    .WithFileName(MapFileName(report))
                    .WithColumn(BrokenExternalReferencesReportColumns.Origin, MapBrokenLinkOriginType)
                    .WithColumn(BrokenExternalReferencesReportColumns.OriginObjectName, MapBrokenLinkOriginName)
                    .WithColumn(BrokenExternalReferencesReportColumns.RefName, MapReferenceName)
                    .WithColumn(BrokenExternalReferencesReportColumns.ErrorCategory, MapErrorCategory)
                    .WithColumn(BrokenExternalReferencesReportColumns.ErrorCode, MapErrorCode)
                    .WithColumn(BrokenExternalReferencesReportColumns.Url, link => link.ValueOfCheckedUrl);

            csvResponseBuilder = report
                .GetBrokenLinks()
                .Aggregate(csvResponseBuilder, (builder, brokenLink) => builder.WithRow(brokenLink));

            return csvResponseBuilder.Build();
        }

        private static string MapFileName(BrokenExternalReferencesReport report)
        {
            return $"kitos_external_references_report-{report.Created:yyyy-MM-dd}.csv";
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
    }
}