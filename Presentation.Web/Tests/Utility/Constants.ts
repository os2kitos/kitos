// Since app uses namespaces (which is fine for smaller .ts apps) we are unable to import this class which is made using modules
// Using modules is preferable in larger applications as this provides modularity and maintainability as there is no
// global namespace pollution.

// This class cannot currently be added to the app project

class constants {
    readonly kendoSystemNameHeader = "systemNameKendoHeader";
    readonly kendoSystemNameObjects = "systemNameKendoObject";
    readonly kendoSystemButtonCreate = "createITSystemButton";
    readonly kendoContractNameHeader = "contractNameHeader";
    readonly kendoContractNameObjects = "contractNameObject";
    readonly kendoButtonResetFilter = "resetFilterButton";
    readonly kendoButtonSaveFilter = "saveFilterButton";
    readonly kendoButtonUseFilter = "useFilterButton";
    readonly kendoButtonDeleteFilter = "removeFilterButton";
    readonly kendoContractButtonCreateContract = "CreateContractButton";
    readonly kendoCatalogNameHeader = "catalogNameHeader";
    readonly kendoCatalogNameObjects = "catalogNameObject";
    readonly kendoCatalogUsageHeader = "catalogUsageHeader";
    readonly kendoCatalogUsageObjects = "catalogUsageObject";
    readonly kendoResetFilter = "resetFilterButton";
    readonly kendoSaveFilter = "saveFilterButton";
    readonly kendoUseFilter = "useFilterButton";
    readonly kendoRemoveFilter = "removeFilterButton";

    //Enviromental veriable
    readonly defaultItContractName = "DefaultTestItContract";
    readonly contractNameVariable = "Name";
    readonly defaultCatalog = "katalog123";
    readonly nameOfSystemInput = "nameOfItSystemInput";
    readonly saveCatalogButton = "itCatalogSaveButton";
    readonly loginFormField = "loginFormField";

    readonly navigationOrganizationButton = "organizationButton";
    readonly navigationProjectButton = "projectButton";
    readonly navigationSystemButton = "systemButton";
    readonly navigationContractButton = "contractButton";
    readonly navigationReportsButton = "reportButton";

    readonly navigationDropdown = "dropdown-button";
    readonly navigationDropdownMyProfile = "myProfileAnchor";
    readonly navigationDropdownLocalAdmin = "localAdminAnchor";
    readonly navigationDropdownGlobalAdmin = "globalAdminAnchor";
    readonly navigationDropdownChangeOrg = "changeOrganizationAnchor";
    readonly navigationDropdownLogOut = "logOutAnchor";
}

export = constants;

