import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder,
    saveFilter: protractor.ElementFinder,
    useFilter: protractor.ElementFinder,
    deleteFilter: protractor.ElementFinder,
    createContract: protractor.ElementFinder,
    systemCatalogCreate: protractor.ElementFinder
    deleteFilter: protractor.ElementFinder,
    editReference: protractor.ElementFinder,
    editSaveReference: protractor.ElementFinder
};

type InputFields =
{
        referenceDocTitle: protractor.ElementFinder,
        referenceDocId: protractor.ElementFinder,
        referenceDocUrl: protractor.ElementFinder
}

type FieldsForms = {
  
};

type ColumnHeaders = {
    systemName: protractor.ElementFinder,
    referenceName: protractor.ElementFinder
    systemName: protractor.ElementFinder,
    contractName: protractor.ElementFinder,
    catalogName: protractor.ElementFinder,
    catalogUsage: protractor.ElementFinder
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder,
    referenceName: protractor.ElementArrayFinder

    systemName: protractor.ElementArrayFinder,
    contractName: protractor.ElementArrayFinder,
    catalogName: protractor.ElementArrayFinder,
    catalogUsage: protractor.ElementArrayFinder
};

var byDataElementType = new CSSLocator().byDataElementType;
var consts = new Constants();

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {
     
        var buttons: HeaderButtons = {
            resetFilter: element(byDataElementType(consts.kendoResetFilter)),
            saveFilter: element(byDataElementType(consts.kendoSaveFilter)),
            useFilter: element(byDataElementType(consts.kendoUseFilter)),
            deleteFilter: element(byDataElementType(consts.kendoRemoveFilter)),
            editReference: element(byDataElementType(consts.kendoReferenceEditButton)),
            editSaveReference: element(byDataElementType(consts.kendoReferenceEditSaveButton))
            resetFilter: element(byDataElementType(consts.kendoButtonResetFilter)),
            saveFilter: element(byDataElementType(consts.kendoButtonSaveFilter)),
            useFilter: element(byDataElementType(consts.kendoButtonUseFilter)),
            deleteFilter: element(byDataElementType(consts.kendoButtonDeleteFilter)),
            createContract: element(byDataElementType(consts.kendoContractButtonCreateContract)),
            systemCatalogCreate: element(byDataElementType(consts.kendoSystemButtonCreate))

        };
        return buttons;
    }

    public inputFields(): InputFields {

        var inputs: InputFields = {
            referenceDocTitle: element(byDataElementType(consts.kendoReferenceFieldTitle)),
            referenceDocId: element(byDataElementType(consts.kendoReferenceFieldId)),
            referenceDocUrl: element(byDataElementType(consts.kendoReferenceFieldUrl))
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

            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader),
            referenceName: kendo.getColumnHeaderClickable(consts.kendoReferencetNameHeader)

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
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects),
            referenceName: kendo.getColumnItemLinks(consts.kendoReferenceNameObjects)
        };
        return columns;
    }
}

class kendoHelper {

    public getColumnHeaderClickable(headerHook: string) {
        return element(byDataElementType(headerHook)).element(by.css("a[class=k-link]"));
    }

    public getColumnItemLinks(itemHook: string) {
        return element.all(byDataElementType(itemHook)).all(by.tagName("a"));
    }
}

export = kendoToolbarWrapper;
