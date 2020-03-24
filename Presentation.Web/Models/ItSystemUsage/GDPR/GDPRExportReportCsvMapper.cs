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
        private static string no = "Nej";
        private static string yes = "Ja";
        private static string dontKnow = "Ved ikke";

        private static string low = "Lav";
        private static string middle = "middel";
        private static string high = "Høj";

        private static string onPremise = "On-premise";
        private static string external = "Eksternt";

        public static HttpResponseMessage CreateReportCsvResponse(IEnumerable<GDPRExportReport> report)
        {
            var csvResponseBuilder = new CsvResponseBuilder<GDPRExportReport>();

            csvResponseBuilder =
                csvResponseBuilder
                    .WithFileName(CreateFileName())
                    .WithColumn(GDPRExportReportColumns.SystemName, x => x.SystemName)
                    .WithColumn(GDPRExportReportColumns.NoData, x => MapBoolean(x.NoData))
                    .WithColumn(GDPRExportReportColumns.PersonalData, x => MapBoolean(x.PersonalData))
                    .WithColumn(GDPRExportReportColumns.SensitiveData, x => MapBoolean(x.SensitiveData))
                    .WithColumn(GDPRExportReportColumns.LegalData, x => MapBoolean(x.LegalData))
                    .WithColumn(GDPRExportReportColumns.ChosenSensitiveData, x => MapSensitiveDataTypes(x.SensitiveDataTypes))
                    .WithColumn(GDPRExportReportColumns.BusinessCritical, x => MapDataOption(x.BusinessCritical))
                    .WithColumn(GDPRExportReportColumns.DataProcessorContract, x => MapBoolean(x.DataProcessorContract))
                    .WithColumn(GDPRExportReportColumns.DataProcessorControl, x => MapDataOption(x.DataProcessorControl))
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
            return input ? yes : no;
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
                    return no;
                case DataOptions.YES:
                    return yes;
                case DataOptions.DONTKNOW:
                    return dontKnow;
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
                    return low;
                case RiskLevel.MIDDLE:
                    return middle;
                case RiskLevel.HIGH:
                    return high;
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
                    return onPremise;
                case HostedAt.EXTERNAL:
                    return external;
                default:
                    return "";
            }
        }
    }
}