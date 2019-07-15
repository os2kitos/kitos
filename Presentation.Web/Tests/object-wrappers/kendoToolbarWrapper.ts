import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder,
    saveFilter: protractor.ElementFinder,
    useFilter: protractor.ElementFinder,
    deleteFilter: protractor.ElementFinder,
    createContract: protractor.ElementFinder,
    systemKatalogCreate: protractor.ElementFinder
};

    

type ColumnHeaders = {
    systemName: protractor.ElementFinder,
    contractName: protractor.ElementFinder,
    catalogName: protractor.ElementFinder,
    catalogUsage: protractor.ElementFinder
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder,
    contractName: protractor.ElementArrayFinder,
    catalogName: protractor.ElementArrayFinder,
    catalogUsage: protractor.ElementArrayFinder
};

var byDataElementType = new CSSLocator().byDataElementType;

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {

        var buttons: HeaderButtons = {
            resetFilter: element(byDataElementType("resetFilter")),
            saveFilter: element(byDataElementType("saveFilter")),
            useFilter: element(byDataElementType("useFilter")),
            deleteFilter: element(byDataElementType("removeFilter")),
            createContract: element(byDataElementType("CreateContract")),
            systemKatalogCreate: element(byDataElementType("CreateITSystemButton"))

        };
        return buttons;
    }

    public columnHeaders(): ColumnHeaders {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnHeaders = {
            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader),
            contractName: kendo.getColumnHeaderClickable(consts.kendoContractNameHeader),
            catalogName: kendo.getColumnHeaderClickable(consts.kendoCatalogNameHeader),
            catalogUsage: kendo.getColumnHeaderClickable(consts.kendoCatalogUsageHeader),

        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnObjects = {
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects),
            contractName: kendo.getColumnItemLinks(consts.kendoContractNameObjects),
            catalogName: kendo.getColumnItemLinks(consts.kendoCatalogNameObjects),
            catalogUsage: kendo.getColumnItemLinks(consts.kendoCatalogUsageObjects),
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
