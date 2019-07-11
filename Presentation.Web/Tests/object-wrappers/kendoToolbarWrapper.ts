import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder,
    saveFilter: protractor.ElementFinder,
    useFilter: protractor.ElementFinder,
    deleteFilter: protractor.ElementFinder
};

type ColumnHeaders = {
    systemName: protractor.ElementFinder
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder

};

var byDataElementType = new CSSLocator().byDataElementType;

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {

        var buttons: HeaderButtons = {
            resetFilter: element(byDataElementType("resetFilter")),
            saveFilter: element(byDataElementType("saveFilter")),
            useFilter: element(byDataElementType("useFilter")),
            deleteFilter: element(byDataElementType("removeFilter"))
        };
        return buttons;
    }

    public columnHeaders(): ColumnHeaders {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnHeaders = {
            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader)
            
        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnObjects = {
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects)
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