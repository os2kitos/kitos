import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder,
    saveFilter: protractor.ElementFinder,
    useFilter: protractor.ElementFinder,
    deleteFilter: protractor.ElementFinder,
    createContract: protractor.ElementFinder,
    systemCatalogCreate: protractor.ElementFinder,
    editReference: protractor.ElementFinder,
    deleteReference: protractor.ElementFinder,
    editSaveReference: protractor.ElementFinder,
    createReference: protractor.ElementFinder
};

type InputFields =
{
    referenceDocTitle: protractor.ElementFinder,
    referenceDocId: protractor.ElementFinder,
    referenceDocUrl: protractor.ElementFinder,
    referenceCreator: protractor.ElementFinder
};

type ColumnHeaders = {
    systemName: protractor.ElementFinder,
    referenceName: protractor.ElementFinder,
    referenceId: protractor.ElementFinder,
    contractName: protractor.ElementFinder,
    catalogName: protractor.ElementFinder,
    catalogUsage: protractor.ElementFinder,
    usedByNameHeader: protractor.ElementFinder,
    userApi: protractor.ElementFinder,
    userEmail: protractor.ElementFinder,
    systemRightsOwner: protractor.ElementFinder
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder,
    referenceName: protractor.ElementArrayFinder,
    referenceId: protractor.ElementArrayFinder,
    contractName: protractor.ElementArrayFinder,
    catalogName: protractor.ElementArrayFinder,
    usedByName: protractor.ElementArrayFinder,
    catalogUsage: protractor.ElementArrayFinder,
    userApi: protractor.ElementArrayFinder,
    UserEmail: protractor.ElementArrayFinder,
    systemRightsOwner: protractor.ElementArrayFinder
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
            systemCatalogCreate: element(byDataElementType(consts.kendoSystemButtonCreate)),
            createReference: element(byDataElementType(consts.kendoCreateReferenceButton)),
            deleteReference: element(byDataElementType(consts.kendoReferenceDeleteButton))

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
            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader),
            contractName: kendo.getColumnHeaderClickable(consts.kendoContractNameHeader),
            catalogName: kendo.getColumnHeaderClickable(consts.kendoCatalogNameHeader),
            catalogUsage: kendo.getColumnHeaderClickable(consts.kendoCatalogUsageHeader),
            referenceName: kendo.getColumnHeaderClickable(consts.kendoReferencetNameHeader),
            referenceId: kendo.getColumnHeaderClickable(consts.kendoReferenceHeaderId),
            userApi: kendo.getUserColumnHeaderClickable(consts.kendoUserApiHeader),
            userEmail: kendo.getColumnHeaderClickable(consts.kendoUserEmailHeader),
            usedByNameHeader: kendo.getColumnHeaderClickable(consts.kendoUsedByHeader),
            systemRightsOwner: kendo.getColumnHeaderClickable(consts.kendoSystemRightsOwnerHeader)

        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();

        var columns: ColumnObjects = {
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects),
            contractName: kendo.getColumnItemLinks(consts.kendoContractNameObjects),
            catalogName: kendo.getColumnItemLinks(consts.kendoCatalogNameObjects),
            catalogUsage: kendo.getColumnItemLinks(consts.kendoCatalogUsageObjects),
            userApi: kendo.getColumnItemLinks(consts.kendoUserApiObject),
            UserEmail: kendo.getColumnItemLinks(consts.kendoUserEmailObject),
            referenceName: kendo.getColumnItemLinks(consts.kendoReferenceNameObjects),
            referenceId: kendo.getColumnItemLinks(consts.kendoReferenceHeaderIdObjects),
            usedByName: kendo.getColumnItemLinks(consts.kendoUsedByObject),
            systemRightsOwner: kendo.getColumnItemLinks(consts.kendoSystemRightsOwnerObject)
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
        return element(byDataElementType(headerHook)).element(by.css("a[class=k-link]"));
    }

    public getUserColumnHeaderClickable(headerHook: string) {
            return element(byDataElementType(headerHook)).element(by.css("a[class=k-header-column-menu]"));
        }

    public getColumnItemLinks(itemHook: string) {
        return element.all(byDataElementType(itemHook)).all(by.tagName("a"));
    }
}

export = kendoToolbarWrapper;
