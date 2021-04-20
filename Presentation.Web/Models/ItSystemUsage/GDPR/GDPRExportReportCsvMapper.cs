using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.Csv;

namespace Presentation.Web.Models.ItSystemUsage.GDPR
{
    public class GDPRExportReportCsvMapper
    {
        private const string No = "Nej";
        private const string Yes = "Ja";
        private const string DontKnow = "Ved ikke";

        private const string Low = "Lav";
        private const string Middle = "Middel";
        private const string High = "Høj";

        private const string OnPremise = "On-premise";
        private const string External = "Eksternt";

        public static HttpResponseMessage CreateReportCsvResponse(IEnumerable<GDPRExportReport> report)
        {
            var csvResponseBuilder = new CsvResponseBuilder<GDPRExportReport>();

            csvResponseBuilder =
                csvResponseBuilder
                    .WithFileName(CreateFileName())
                    .WithColumn(GDPRExportReportColumns.SystemUuid, x => x.SystemUuid)
                    .WithColumn(GDPRExportReportColumns.SystemName, x => x.SystemName)
                    .WithColumn(GDPRExportReportColumns.NoData, x => MapBoolean(x.NoData))
                    .WithColumn(GDPRExportReportColumns.PersonalData, x => MapBoolean(x.PersonalData))
                    .WithColumn(GDPRExportReportColumns.SensitiveData, x => MapBoolean(x.SensitiveData))
                    .WithColumn(GDPRExportReportColumns.LegalData, x => MapBoolean(x.LegalData))
                    .WithColumn(GDPRExportReportColumns.ChosenSensitiveData, x => MapSensitiveDataTypes(x.SensitiveDataTypes))
                    .WithColumn(GDPRExportReportColumns.BusinessCritical, x => MapDataOption(x.BusinessCritical))
                    .WithColumn(GDPRExportReportColumns.DataProcessorContract, x => MapBoolean(x.DataProcessingAgreementConcluded))
                    .WithColumn(GDPRExportReportColumns.LinkToDirectory, x => MapBoolean(x.LinkToDirectory))
                    .WithColumn(GDPRExportReportColumns.RiskAssessment, x => MapDataOption(x.RiskAssessment))
                    .WithColumn(GDPRExportReportColumns.PreRiskAssessment, x => MapRiskLevel(x.PreRiskAssessment))
                    .WithColumn(GDPRExportReportColumns.DPIA, x => MapDataOption(x.DPIA))
                    .WithColumn(GDPRExportReportColumns.HostedAt, x => MapHostedAt(x.HostedAt));

            csvResponseBuilder = report
                .Aggregate(csvResponseBuilder, (builder, gdprReport) => builder.WithRow(gdprReport));

            return csvResponseBuilder.Build();
        }

        private static string CreateFileName()
        {
            return $"kitos_gdpr_report-{DateTime.Now:yyyy-MM-dd}.csv";
        }

        private static string MapBoolean(bool input)
        {
            return input ? Yes : No;
        }

        private static string MapSensitiveDataTypes(IEnumerable<string> input)
        {
            return string.Join(", ", input);
        }

        private static string MapDataOption(DataOptions? input)
        {
            if (!input.HasValue) return "";

            switch (input.Value)
            {
                case DataOptions.NO:
                    return No;
                case DataOptions.YES:
                    return Yes;
                case DataOptions.DONTKNOW:
                    return DontKnow;
                case DataOptions.UNDECIDED:
                    return "";
                default:
                    return "";
            }
        }

        private static string MapRiskLevel(RiskLevel? input)
        {
            if (!input.HasValue) return "";

            switch (input.Value)
            {
                case RiskLevel.LOW:
                    return Low;
                case RiskLevel.MIDDLE:
                    return Middle;
                case RiskLevel.HIGH:
                    return High;
                case RiskLevel.UNDECIDED:
                    return "";
                default:
                    return "";
            }
        }

        private static string MapHostedAt(HostedAt? input)
        {
            if (!input.HasValue) return "";

            switch (input.Value)
            {
                case HostedAt.UNDECIDED:
                    return "";
                case HostedAt.ONPREMISE:
                    return OnPremise;
                case HostedAt.EXTERNAL:
                    return External;
                default:
                    return "";
            }
        }
    }
}