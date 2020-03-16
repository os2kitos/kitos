// Since app uses namespaces (which is fine for smaller .ts apps) we are unable to import this class which is made using modules
// Using modules is preferable in larger applications as this provides modularity and maintainability as there is no
// global namespace pollution.

// This class cannot currently be added to the app project
class Constants {
    readonly kendoSystemNameHeader = "systemNameKendoHeader";
    readonly kendoSystemNameObjects = "systemNameKendoObject";
    readonly kendoReportNameObjects = "reportNameKendoObject";
    readonly kendoProjectNameObjects = "projectNameKendoObject";
    readonly kendoSystemRightsOwnerHeader = "systemRightsOwnerHeader";
    readonly kendoSystemRightsOwnerObject = "systemRightsOwnerObject";
    readonly kendoSystemButtonCreate = "createITSystemButton";
    readonly kendoContractNameHeader = "contractNameHeader";
    readonly kendoContractNameObjects = "contractNameObject";
    readonly kendoButtonResetFilter = "resetFilterButton";
    readonly kendoButtonSaveFilter = "saveFilterButton";
    readonly kendoButtonUseFilter = "useFilterButton";
    readonly kendoButtonDeleteFilter = "removeFilterButton";
    readonly kendoContractButtonCreateContract = "createContractButton";
    readonly kendoProjectButtonCreateProject = "createProjectButton";
    readonly kendoReportButtonCreateReport = "createReportButton";
    readonly kendoCatalogNameHeader = "catalogNameHeader";
    readonly kendoCatalogNameObjects = "catalogNameObject";
    readonly kendoCatalogUsageHeader = "catalogUsageHeader";
    readonly kendoCatalogUsageObjects = "catalogUsageObject";
    readonly kendoCreateReferenceButton = "createReferenceButton";
    readonly kendoUsedByHeader = "usedByNameHeader";
    readonly kendoUsedByObject = "usedByNameObject";

    readonly kendoCatalogNameFilter = "Name";

    readonly kendoUserEmailHeader = "userHeaderEmail";
    readonly kendoUserEmailObject = "userEmailObject";
    readonly kendoUserApiHeader = "userHeader";
    readonly kendoUserApiObject = "userObject";
    readonly kendoResetFilter = "resetFilterButton";
    readonly kendoSaveFilter = "saveFilterButton";
    readonly kendoUseFilter = "useFilterButton";
    readonly kendoRemoveFilter = "removeFilterButton";
    readonly kendoUserDeleteButton = "userDeleteButton";

    readonly kendoRelationCountObject = "relationCountObject";

    //Environmental variables
    readonly defaultSystemUsageName = "DefaultTestItSystem";
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
    readonly migrationOrgNameToMove = "MigrationMoveOrgName";

    //interfaceInoputConstants
    readonly interfaceNameInput = "InterfaceNameInput";
    readonly interfaceIdInput = "InterfaceIdInput";
    readonly interfaceVersionInput = "InterfaceVersionInput";
    readonly interfaceExhibitInput = "InterfaceExhibitInput";
    readonly interfaceBelongsToInput = "InterfaceBelongsToInput";
    readonly interfaceAccesAbilityInput = "InterfaceAccesAbilityInput";
    readonly interfaceInterfaceInput = "InterfaceInterfaceInput";
    readonly interfaceDescriptionInput = "InterfaceDescriptionInput";
    readonly interfaceNoteInput = "InterfaceNoteInput";
    readonly interfaceDescriptionLinkInput = "InterfaceDescriptionLinkInput";
    readonly interfaceDataInput = "InterfaceDataInput";
    readonly interfaceDataTypeInput = "InterfaceDataTypeInput";
    readonly interfaceDataTableInput = "InterfaceDataInput";
    readonly interfaceNewRowButton = "InterfaceNewRowButton";
    readonly interfaceBelongsToReadonly = "InterfaceBelongsToReadonly";

    readonly interfaceSelectExhibit = "s2id_interface-exposed-by";
    readonly interfaceSelectBelongs = "s2id_belongs-to";
    readonly interfaceSelectAccess = "s2id_interface-access";
    readonly interfaceSelectInterface = "s2id_interface-interface";
    readonly interfaceSelectTableDataType = "s2id_interface-data-type";

    readonly navigationOrganizationButton = "organizationButton";
    readonly navigationProjectButton = "projectButton";
    readonly navigationSystemButton = "systemButton";
    readonly navigationRemoveSystemUsageButton = "removeSystemUsageButton";
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
    readonly kleUpdateButton = "KLEUpdateButton";
    readonly KleDownloadAnchor = "KLEDownloadAnchor";

    //Local System 
    readonly createRelationButton = "create-Relation";
    readonly referenceInputField = "Reference";
    readonly descriptionInputField = "description";
    readonly saveButtonText = "Gem";

    readonly systemUsageHeaderName = "systemUsageHeaderName";

    readonly mainLocalId = "sysId";
    readonly mainCallName = "localcallname";
    readonly mainNote = "note";
    readonly mainVersion = "version";
    readonly mainOwner = "usage-owner";
    readonly mainSystemName = "system-name";
    readonly mainParentName = "system-parent";
    readonly mainPreviousName = "system-previousname";
    readonly mainBelongsTo = "system-belongs-to";
    readonly mainAccess = "system-access";
    readonly mainDescription = "system-description";
    readonly mainReferences = "system-referencer";
    readonly mainKLE = "system-kle";
    readonly mainBusinessType = "system-business-type";
    readonly mainArchive = "archiveDuty";
    readonly mainUUID = "system-uuid";

    readonly relationDescriptionField = "relationDescription";
    readonly relationReferenceField = "relationReference";
    readonly relationFrequencyTypeField = "relationFrequencyType";

    readonly usedByRelationDescriptionField = "usedByRelationDescription";
    readonly usedByRelationReferenceField = "usedByRelationReference";
    readonly usedByRelationFrequencyTypeField = "usedByRelationFrequencyType";

    readonly dataLevelTypeNoneCheckbox = "dataLevelTypeNoneCheckbox";
    readonly dataLevelTypeRegularCheckbox = "dataLevelTypeRegularCheckbox";
    readonly dataLevelTypeSensitiveCheckbox = "dataLevelTypeSensitiveCheckbox";
    readonly dataLevelTypeLegalCheckbox = "dataLevelTypeLegalCheckbox";

    readonly defaultPersonalSensitivData1 = "sensitivepersonalDataCheckbox-TestFølsomData";
    readonly defaultPersonalSensitivData2 = "sensitivepersonalDataCheckbox-FølsomDataTest";

    //Local admin

    readonly itProjectIncludeModuleInput = "project-include";
    
}
export = Constants;

