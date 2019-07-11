import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder, saveFilter: protractor.ElementFinder, useFilter: protractor.ElementFinder, deleteFilter: protractor.ElementFinder,
    createContract: protractor.ElementFinder,

    }

type ColumnHeaders = {
    systemName: protractor.ElementFinder,contractName: protractor.ElementFinder,
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder, contractName: protractor.ElementArrayFinder,

};
var byHook = new CSSLocator().byDataHook;

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {

        var buttons: HeaderButtons = {
            resetFilter: element(byHook("resetFilter")),
            saveFilter: element(byHook("saveFilter")),
            useFilter: element(byHook("useFilter")),
            deleteFilter: element(byHook("removeFilter")),
            createContract: element(byHook("createContract")),


        };
        return buttons;
    }

    public columnHeaders(): ColumnHeaders {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnHeaders = {
            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader),
            contractName: kendo.getColumnHeaderClickable(consts.kendoContractNameHeader),

        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnObjects = {
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects),
            contractName: kendo.getColumnItemLinks(consts.kendoContractNameObjects),
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
