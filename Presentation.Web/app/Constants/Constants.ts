module Kitos.Constants {

    export class ApplicationStateId {
        static readonly Index = "index";
        static readonly SystemUsageOverview = "it-system.overview";
        static readonly SystemCatalog = "it-system.catalog";
        static readonly ContractOverview = "it-contract.overview";
        static readonly DataProcessingRegistrationOverview = "data-processing.overview";
        static readonly OrganizationOverview = "organization.overview";
        static readonly InterfaceCatalog = "it-system.interfaceCatalog";
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
        static readonly DefaultTitle = "Eksportér til Excel";
        static readonly DataKey = "kendoDropDownList";

        static readonly SelectAllId = "exportExcelAll";
        static readonly SelectAllValue = "Alle kolonner";
        static readonly SelectOnlyVisibleId = "exportExcelOnlyVisible";
        static readonly SelectOnlyVisibleValue = "Viste kolonner";

        static readonly ChooseWhichExcelOptionId = "chooseWhichExcelSelector";
        static readonly ChooseWhichExcelOptionValue = "- - - Vælg hvordan - - -";
    }

    export class CustomFilterDropdown {
        static readonly Id = "combinedFilterButtonsDropDownList";
        static readonly DefaultTitle = "Choose filter option";
        static readonly DataKey = "kendoDropDownList";

        static readonly SaveFilter = {
            Id: "saveFilter",
            Text: "Gem filter",
        };

        static readonly UseFilter = {
            Id: "useFilter",
            Text: "Anvend filter",
        };

        static readonly DeleteFilter = {
            Id: "deleteFilter",
            Text: "Slet filter"
        };

        static readonly DefaultOptionFilter = {
            Id: "defaultFitlerOption",
            Text: "- - - Vælg hvordan - - -"
        };

        static readonly SelectAllId = "exportExcelAll";
        static readonly SelectAllValue = "Alle kolonner";
        static readonly SelectOnlyVisibleId = "exportExcelOnlyVisible";
        static readonly SelectOnlyVisibleValue = "Viste kolonner";

        static readonly ChooseWhichExcelOptionId = "chooseWhichExcelSelector";
        static readonly ChooseWhichExcelOptionValue = "- - - Vælg hvordan - - -";
    }
}