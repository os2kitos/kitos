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
            referenceName: kendo.getColumnHeaderClickable(consts.kendoReferencetNameHeader)

        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();
        

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
