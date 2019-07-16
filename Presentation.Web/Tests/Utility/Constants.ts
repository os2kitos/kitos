// Since app uses namespaces (which is fine for smaller .ts apps) we are unable to import this class which is made using modules
// Using modules is preferable in larger applications as this provides modularity and maintainability as there is no
// global namespace pollution.

// This class cannot currently be added to the app project

class constants {
    readonly kendoSystemNameHeader = "systemNameKendoHeader";
    readonly kendoSystemNameObjects = "systemNameKendoObject";
    readonly kendoSystemButtonCreate = "CreateITSystemButton";
    readonly kendoContractNameHeader = "contractNameHeader";
    readonly kendoContractNameObjects = "contractNameObject";
    readonly kendoButtonResetFilter = "resetFilterButton";
    readonly kendoButtonSaveFilter = "saveFilterButton";
    readonly kendoButtonUseFilter = "useFilterButton";
    readonly kendoButtonDeleteFilter = "removeFilterButton";
    readonly kendoContractButtonCreateContract = "CreateContractButton";
    readonly kendoCatalogNameHeader = "CatalogNameHeader";
    readonly kendoCatalogNameObjects = "CatalogNameObject";
    readonly kendoCatalogUsageHeader = "CatalogUsageHeader";
    readonly kendoCatalogUsageObjects = "CatalogUsageObject";
}

export = constants;

