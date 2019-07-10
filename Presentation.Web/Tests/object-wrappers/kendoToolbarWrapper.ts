import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder, saveFilter: protractor.ElementFinder, useFilter: protractor.ElementFinder, deleteFilter: protractor.ElementFinder
};

type ClickableColumnHeaders = {
    systemName: protractor.ElementFinder
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder

};

var byHook = new CSSLocator().byDataHook;

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {
        var buttons: HeaderButtons = {
            resetFilter: element(byHook("resetFilter")),
            saveFilter: element(byHook("saveFilter")),
            useFilter: element(byHook("useFilter")),
            deleteFilter: element(byHook("removeFilter"))
        };
        return buttons;
    }

    public columnHeaders(): ClickableColumnHeaders {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ClickableColumnHeaders = {
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
        return element(byHook(headerHook)).element(by.css("a[class=k-link]"));
    }

    public getColumnItemLinks(itemHook: string) {
        return element.all(byHook(itemHook)).all(by.tagName("a"));
    }
}

export = kendoToolbarWrapper;
