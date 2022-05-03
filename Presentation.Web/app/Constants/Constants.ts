module Kitos.Constants {

    export class SRef {
        static readonly SystemUsageOverview = "it-system.overview";
        static readonly ContractOverview = "it-contract.overview";
        static readonly ContractPlanOverview = "it-contract.plan";
        static readonly ProjectOverview = "it-project.overview";
        static readonly DataProcessingRegistrationOverview = "data-processing.overview";
    }

    export class CSRF {
        static readonly CSRFCookie = "XSRF-TOKEN";
        static readonly CSRFHeader = "X-XSRF-TOKEN";
        static readonly HiddenFieldName = "__RequestVerificationToken";
    }

    export class Archiving {
        static readonly ReadMoreUri = "https://www.sa.dk/da/offentlig-forvaltning/kommuner-og-regioner/bevaring-kassation-it-systemer/";
    }

    export class Select2 {
        static readonly EmptyField = "\u00a0";
    }

    export class DateFormat {
        static readonly DanishDateFormat = "DD-MM-YYYY";

    }

    export class ExcelExportDropdown {
        static readonly Id = "excelExportSelector";
        static readonly DefaultValue = "0";
        static readonly DefaultTitle = "Eksportér til Excel";
        static readonly DataKey = "kendoDropDownList";
        static readonly DefaultPosition = "pull-right";

        static readonly SelectAllId = "exportExcelAll";
        static readonly SelectAllValue = "Alt data";
        static readonly SelectOnlyVisibleId = "exportExcelOnlyVisible";
        static readonly SelectOnlyVisibleValue = "Kun de viste kolonner";
    }
}