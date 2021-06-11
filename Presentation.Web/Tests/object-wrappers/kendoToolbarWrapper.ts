import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder,
    saveFilter: protractor.ElementFinder,
    useFilter: protractor.ElementFinder,
    deleteFilter: protractor.ElementFinder,
    createContract: protractor.ElementFinder,
    createProject: protractor.ElementFinder,
    createReport: protractor.ElementFinder,
    systemCatalogCreate: protractor.ElementFinder,
    createDpa: protractor.ElementFinder,
    editReference: protractor.ElementFinder,
    deleteReference: protractor.ElementFinder,
    editSaveReference: protractor.ElementFinder,
    createReference: protractor.ElementFinder,
    saveFilterToOrg: protractor.ElementFinder
};

type InputFields =
{
    referenceDocTitle: protractor.ElementFinder,
    referenceDocId: protractor.ElementFinder,
    referenceDocUrl: protractor.ElementFinder,
    referenceCreator: protractor.ElementFinder,
};

type ColumnHeaders = {
    referenceName: protractor.ElementFinder,
    referenceId: protractor.ElementFinder,
    contractName: protractor.ElementFinder,
    catalogName: protractor.ElementFinder,
    catalogUsage: protractor.ElementFinder,
    usedByNameHeader: protractor.ElementFinder,
    userApi: protractor.ElementFinder,
    userRightsHolderAccess: protractor.ElementFinder,
    userStakeHolderAccess : protractor.ElementFinder,
    userEmail: protractor.ElementFinder,
};

type ColumnObjects = {
    projectName: protractor.ElementArrayFinder,
    systemName: protractor.ElementArrayFinder,
    reportName: protractor.ElementArrayFinder,
    referenceName: protractor.ElementArrayFinder,
    referenceId: protractor.ElementArrayFinder,
    contractName: protractor.ElementArrayFinder,
    catalogName: protractor.ElementArrayFinder,
    interfaceName: protractor.ElementArrayFinder,
    dpaName: protractor.ElementArrayFinder,
    usedByName: protractor.ElementArrayFinder,
    catalogUsage: protractor.ElementArrayFinder,
    userApi: protractor.ElementArrayFinder,
    UserEmail: protractor.ElementArrayFinder,
    systemRightsOwner: protractor.ElementArrayFinder,
    activationToggle: protractor.ElementArrayFinder,
};

var byDataElementType = new CSSLocator().byDataElementType;
var consts = new Constants();

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {
     
        var buttons: HeaderButtons = {
            editReference: element(byDataElementType(consts.kendoReferenceEditButton)),
            editSaveReference: element(byDataElementType(consts.kendoReferenceEditSaveButton)),
            resetFilter: element(byDataElementType(consts.kendoButtonResetFilter)),
            saveFilter: element(byDataElementType(consts.kendoButtonSaveFilter)),
            useFilter: element(byDataElementType(consts.kendoButtonUseFilter)),
            deleteFilter: element(byDataElementType(consts.kendoButtonDeleteFilter)),
            createContract: element(byDataElementType(consts.kendoContractButtonCreateContract)),
            createProject: element(byDataElementType(consts.kendoProjectButtonCreateProject)),
            createReport: element(byDataElementType(consts.kendoReportButtonCreateReport)),
            systemCatalogCreate: element(byDataElementType(consts.kendoSystemButtonCreate)),
            createDpa: element(byDataElementType(consts.kendoDpaButtonCreate)),
            createReference: element(byDataElementType(consts.kendoCreateReferenceButton)),
            deleteReference: element(byDataElementType(consts.kendoReferenceDeleteButton)),
            saveFilterToOrg: element(byDataElementType(consts.filterOrgButton))

        };
        return buttons;
    }

    public inputFields(): InputFields {

        var inputs: InputFields = {
            referenceDocTitle: element(byDataElementType(consts.kendoReferenceFieldTitle)),
            referenceDocId: element(byDataElementType(consts.kendoReferenceFieldId)),
            referenceDocUrl: element(byDataElementType(consts.kendoReferenceFieldUrl)),
            referenceCreator: element(byDataElementType(consts.createReferenceForm))
        };
        return inputs;
    }

    public columnHeaders(): ColumnHeaders {
        var kendo = new kendoHelper();

        var columns: ColumnHeaders = {
            contractName: kendo.getColumnHeaderClickable(consts.kendoContractNameHeader),
            catalogName: kendo.getColumnHeaderClickable(consts.kendoCatalogNameHeader),
            catalogUsage: kendo.getColumnHeaderClickable(consts.kendoCatalogUsageHeader),
            referenceName: kendo.getColumnHeaderClickable(consts.kendoReferencetNameHeader),
            referenceId: kendo.getColumnHeaderClickable(consts.kendoReferenceHeaderId),
            userApi: kendo.getColumnHeader(consts.kendoUserApiHeader),
            userEmail: kendo.getColumnHeaderClickable(consts.kendoUserEmailHeader),
            usedByNameHeader: kendo.getColumnHeaderClickable(consts.kendoUsedByHeader),
            userRightsHolderAccess: kendo.getColumnHeader(consts.kendoUserRightsHolderHeader),
            userStakeHolderAccess: kendo.getColumnHeader(consts.kendoUserStakeHolderHeader)

        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();

        var columns: ColumnObjects = {
            projectName: kendo.getColumnItemLinks(consts.kendoProjectNameObjects),
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects),
            reportName: kendo.getColumnItemLinks(consts.kendoReportNameObjects),
            contractName: kendo.getColumnItemLinks(consts.kendoContractNameObjects),
            catalogName: kendo.getColumnItemLinks(consts.kendoCatalogNameObjects),
            interfaceName: kendo.getColumnItemLinks(consts.kendoInterfaceNameObjects),
            dpaName: kendo.getColumnItemLinks(consts.kendoDpaNameObjects),
            catalogUsage: kendo.getColumnItemLinks(consts.kendoCatalogUsageObjects),
            userApi: kendo.getColumnItemLinks(consts.kendoUserApiObject),
            UserEmail: kendo.getColumnItemLinks(consts.kendoUserEmailObject),
            referenceName: kendo.getColumnItemLinks(consts.kendoReferenceNameObjects),
            referenceId: kendo.getColumnItemLinks(consts.kendoReferenceHeaderIdObjects),
            usedByName: kendo.getColumnItemLinks(consts.kendoUsedByObject),
            systemRightsOwner: kendo.getColumnItemLinks(consts.kendoSystemRightsOwnerObject),
            activationToggle: kendo.getButtons(consts.kendoCatalogUsageObjects)
        };
        return columns;
    }

    public getFilteredColumnElement(column: protractor.ElementArrayFinder, textValue: string): protractor.ElementArrayFinder {
        var test = column.filter((elem) => {
            return elem.getText().then((val) => {
                if (val === textValue) {
                    return elem;
                }
            });
        });
        return test;
    }
}

class kendoHelper {

    public getColumnHeaderClickable(headerHook: string) {
        return this.getColumnHeader(headerHook).element(by.css("a[class=k-link]"));
    }

    public getUserColumnHeaderMenu(headerHook: string) {
        return this.getColumnHeader(headerHook).element(by.css("a[class=k-header-column-menu]"));
    }

    public getColumnHeader(headerHook: string) {
        return element(byDataElementType(headerHook));
    }

    public getColumnItemLinks(itemHook: string) {
        return element.all(byDataElementType(itemHook)).all(by.tagName("a"));
    }

    public getButtons(itemHook: string) {
        return element.all(byDataElementType(itemHook)).all(by.tagName("button"));
    }
}

export = kendoToolbarWrapper;
