using Presentation.Web.Models.Csv;

namespace Presentation.Web.Models.Qa
{
    public class BrokenExternalReferencesReportColumns
    {
        public static CsvColumnIdentity Origin = new CsvColumnIdentity(nameof(Origin), "Oprindelse");
        public static CsvColumnIdentity OriginObjectName = new CsvColumnIdentity(nameof(OriginObjectName), "Navn");
        public static CsvColumnIdentity RefName = new CsvColumnIdentity(nameof(RefName), "Referencenavn");
        public static CsvColumnIdentity ErrorCategory = new CsvColumnIdentity(nameof(ErrorCategory), "Fejlkategori");
        public static CsvColumnIdentity ErrorCode = new CsvColumnIdentity(nameof(ErrorCode), "Fejlkode");
        public static CsvColumnIdentity Url = new CsvColumnIdentity(nameof(Url), "Url");
    }
}