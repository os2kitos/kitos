// Since app uses namespaces (which is fine for smaller .ts apps) we are unable to import this class which is made using modules
// Using modules is preferable in larger applications as this provides modularity and maintainability as there is no
// global namespace pollution.

// This class cannot currently be added to the app project

class constants {
    readonly kendoSystemNameHeader = "systemNameKendoHeader";
    readonly kendoSystemNameObjects = "systemNameKendoObject";
    readonly kendoContractNameHeader = "contractNameHeader";
    readonly kendoContractNameObjects = "contractName";
    readonly kendoCatalogNameHeader = "CatalogNameHeader";
    readonly kendoCatalogNameObjects = "CatalogName";
    readonly kendoCatalogUsageHeader = "CatalogUsageHeader";
    readonly kendoCatalogUsageObjects = "CatalogUsage";
}

export = constants;

