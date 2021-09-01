using Presentation.Web.Models.Application.Csv;

namespace Presentation.Web.Models.API.V1.ItSystemUsage.GDPR
{
    public class GDPRExportReportColumns
    {
        public static CsvColumnIdentity SystemUuid = new CsvColumnIdentity(nameof(SystemUuid), "UUID");
        public static CsvColumnIdentity SystemName = new CsvColumnIdentity(nameof(SystemName), "Navn");
        public static CsvColumnIdentity NoData = new CsvColumnIdentity(nameof(NoData), "Ingen persondata");
        public static CsvColumnIdentity PersonalData = new CsvColumnIdentity(nameof(PersonalData), "Almindelige persondata");
        public static CsvColumnIdentity SensitiveData = new CsvColumnIdentity(nameof(SensitiveData), "Følsomme persondata");
        public static CsvColumnIdentity ChosenSensitiveData = new CsvColumnIdentity(nameof(ChosenSensitiveData), "Valgte følsomme persondata");
        public static CsvColumnIdentity LegalData = new CsvColumnIdentity(nameof(LegalData), "Straffesager og lovovertrædelser");
        public static CsvColumnIdentity BusinessCritical = new CsvColumnIdentity(nameof(BusinessCritical), "Forretningskritisk IT-System");
        public static CsvColumnIdentity DataProcessorContract = new CsvColumnIdentity(nameof(DataProcessorContract), "Databehandleraftale");
        public static CsvColumnIdentity DataProcessorControl = new CsvColumnIdentity(nameof(DataProcessorControl), "Tilsyn/kontrol af databehandleren");
        public static CsvColumnIdentity LinkToDirectory = new CsvColumnIdentity(nameof(LinkToDirectory), "Link til fortegnelse");
        public static CsvColumnIdentity RiskAssessment = new CsvColumnIdentity(nameof(RiskAssessment), "Foretaget risikovurdering");
        public static CsvColumnIdentity PreRiskAssessment = new CsvColumnIdentity(nameof(PreRiskAssessment), "Hvad viste seneste risikovurdering");
        public static CsvColumnIdentity DPIA = new CsvColumnIdentity(nameof(DPIA), "Gennemført DPIA / Konsekvensanalyse");
        public static CsvColumnIdentity HostedAt = new CsvColumnIdentity(nameof(HostedAt), "IT-Systemet driftes");
    }
}