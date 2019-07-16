import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder,
    saveFilter: protractor.ElementFinder,
    useFilter: protractor.ElementFinder,
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

type ColumnHeaders = {
    systemName: protractor.ElementFinder,
    referenceName: protractor.ElementFinder
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder,
    referenceName: protractor.ElementArrayFinder

};

var byDataElementType = new CSSLocator().byDataElementType;

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {

        var buttons: HeaderButtons = {
            resetFilter: element(byDataElementType("resetFilter")),
            saveFilter: element(byDataElementType("saveFilter")),
            useFilter: element(byDataElementType("useFilter")),
            deleteFilter: element(byDataElementType("removeFilter")),
            editReference: element(byDataElementType("EditReference")),
            editSaveReference: element(byDataElementType("editSaveReference"))
        };
        return buttons;
    }

    public inputFields(): InputFields {

        var inputs: InputFields = {
            referenceDocTitle: element(byDataElementType("referenceDocTitle")),
            referenceDocId: element(byDataElementType("referenceDocId")),
            referenceDocUrl: element(byDataElementType("referenceDocUrl"))
        };
        return inputs;
    }

    public columnHeaders(): ColumnHeaders {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnHeaders = {
            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader),
            referenceName: kendo.getColumnHeaderClickable(consts.kendoReferencetNameHeader)

        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnObjects = {
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
