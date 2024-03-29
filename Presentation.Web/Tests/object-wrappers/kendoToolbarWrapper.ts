import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    createContract: protractor.ElementFinder,
    systemCatalogCreate: protractor.ElementFinder,
    createDpa: protractor.ElementFinder,
    editReference: protractor.ElementFinder,
    editSaveReference: protractor.ElementFinder,
    createReference: protractor.ElementFinder,
    deleteReference: protractor.ElementFinder,
    localAdminDropdownReference: protractor.ElementFinder,
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
    userStakeHolderAccess: protractor.ElementFinder,
    userEmail: protractor.ElementFinder,
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder,
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
            createContract: element(byDataElementType(consts.kendoContractButtonCreateContract)),
            systemCatalogCreate: element(byDataElementType(consts.kendoSystemButtonCreate)),
            createDpa: element(byDataElementType(consts.kendoDpaButtonCreate)),
            createReference: element(byDataElementType(consts.kendoCreateReferenceButton)),
            deleteReference: element(byDataElementType(consts.kendoReferenceDeleteButton)),
            localAdminDropdownReference: element(byDataElementType(consts.localAdminDropdown))
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
            userStakeHolderAccess: kendo.getColumnHeader(consts.kendoUserStakeHolderHeader),

        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();

        var columns: ColumnObjects = {
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects),
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
                return null;
            });
        });
        return test;
    }

    public getAnyColumnElement(column: protractor.ElementArrayFinder): protractor.ElementArrayFinder {
        var test = column.filter((elem) => {
            return elem.getText().then((val) => {
                if (val !== null) {
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
