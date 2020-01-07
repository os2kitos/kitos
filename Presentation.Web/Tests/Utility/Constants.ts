// Since app uses namespaces (which is fine for smaller .ts apps) we are unable to import this class which is made using modules
// Using modules is preferable in larger applications as this provides modularity and maintainability as there is no
// global namespace pollution.

// This class cannot currently be added to the app project
class Constants {
    readonly kendoSystemNameHeader = "systemNameKendoHeader";
    readonly kendoSystemNameObjects = "systemNameKendoObject";
    readonly kendoSystemButtonCreate = "createITSystemButton";
    readonly kendoContractNameHeader = "contractNameHeader";
    readonly kendoContractNameObjects = "contractNameObject";
    readonly kendoButtonResetFilter = "resetFilterButton";
    readonly kendoButtonSaveFilter = "saveFilterButton";
    readonly kendoButtonUseFilter = "useFilterButton";
    readonly kendoButtonDeleteFilter = "removeFilterButton";
    readonly kendoContractButtonCreateContract = "createContractButton";
    readonly kendoCatalogNameHeader = "catalogNameHeader";
    readonly kendoCatalogNameObjects = "catalogNameObject";
    readonly kendoCatalogUsageHeader = "catalogUsageHeader";
    readonly kendoCatalogUsageObjects = "catalogUsageObject";
    readonly kendoCreateReferenceButton = "createReferenceButton";
    readonly kendoUsedByHeaderObject = "usedByNameHeader";
    readonly kendoUsedByObject = "usedByNameObject";

    readonly kendoUserEmailHeader = "userHeaderEmail";
    readonly kendoUserEmailObject = "userEmailObject";
    readonly kendoUserApiHeader = "userHeader";
    readonly kendoUserApiObject = "userObject";
    readonly kendoResetFilter = "resetFilterButton";
    readonly kendoSaveFilter = "saveFilterButton";
    readonly kendoUseFilter = "useFilterButton";
    readonly kendoRemoveFilter = "removeFilterButton";
    readonly kendoUserDeleteButton = "userDeleteButton";

    //Environmental variables
    readonly defaultItContractName = "DefaultTestItContract";
    readonly contractNameVariable = "Name";
    readonly nameOfSystemInput = "nameOfItSystemInput";
    readonly saveCatalogButton = "itCatalogSaveButton";
    readonly toggleActivatingSystem = "toggleActivatingSystem";
    readonly moveSystemButton = "migrateItSystem";
    readonly consequenceButton = "consequenceButton";
    readonly startMigrationButton = "startMigration";
    readonly loginFormField = "loginFormField";
    readonly catalogCreateForm = "catalogCreateForm";
    readonly createReferenceForm = "referenceCreatorForm";
    readonly validUrl = "https://strongminds.dk/";
    readonly invalidUrl = "invalidUrl";
    readonly refId = "1337";
    readonly refTitle = "STRONGMINDS";

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
    readonly kendoReferencetNameHeader = "referenceNameHeader";
    readonly kendoReferenceNameObjects = "referenceObject";

    readonly kendoReferenceHeaderId = "referenceHeaderId";
    readonly kendoReferenceHeaderIdObjects = "referenceIdObject";
    readonly kendoReferenceEditButton = "editReference";
    readonly kendoReferenceDeleteButton = "deleteReference";
    readonly kendoReferenceEditSaveButton = "editSaveReference";
    readonly kendoReferenceFieldTitle = "referenceDocTitle";
    readonly kendoReferenceFieldId = "referenceDocId";
    readonly kendoReferenceFieldUrl = "referenceDocUrl";

    readonly kendoSystemDeleteButton = "deleteSystemButton";

    readonly kleStatusLabel = "KLEStatusLabel";
    readonly kleChangesButton = "KLEChangesButton";
    readonly kleUpdateButton = "UpdateKLEButton";

}

export = Constants;

