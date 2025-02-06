using Presentation.Web.Models.Application.Csv;

namespace Presentation.Web.Models.API.V2.Internal.Response.QA
{
    public class BrokenExternalReferencesReportColumnsV2
    {
        public static CsvColumnIdentity Origin = new(nameof(Origin), "Oprindelse");
        public static CsvColumnIdentity OriginObjectName = new(nameof(OriginObjectName), "Navn");
        public static CsvColumnIdentity RefName = new(nameof(RefName), "Referencenavn");
        public static CsvColumnIdentity ErrorCategory = new(nameof(ErrorCategory), "Fejlkategori");
        public static CsvColumnIdentity ErrorCode = new(nameof(ErrorCode), "Fejlkode");
        public static CsvColumnIdentity Url = new(nameof(Url), "Url");
    }
}