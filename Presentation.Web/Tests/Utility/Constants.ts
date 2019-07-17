// Since app uses namespaces (which is fine for smaller .ts apps) we are unable to import this class which is made using modules
// Using modules is preferable in larger applications as this provides modularity and maintainability as there is no
// global namespace pollution.

// This class cannot currently be added to the app project

class constants {
    readonly kendoSystemNameHeader = "systemNameKendoHeader";
    readonly kendoSystemNameObjects = "systemNameKendoObject";
    readonly kendoContractNameHeader = "contractNameHeader";
    readonly kendoContractNameObjects = "contractName";
    readonly kendoResetFilter = "resetFilterButton";
    readonly kendoSaveFilter = "saveFilterButton";
    readonly kendoUseFilter = "useFilterButton";
    readonly kendoRemoveFilter = "removeFilterButton";

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

    readonly kendoReferenceEditButton = "EditReference";
    readonly kendoReferenceEditSaveButton = "editSaveReference";

    readonly kendoReferenceFieldTitle = "referenceDocTitle";
    readonly kendoReferenceFieldId = "referenceDocId";
    readonly kendoReferenceFieldUrl = "referenceDocUrl";



}

export = constants;

