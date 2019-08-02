import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../Utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder,
    saveFilter: protractor.ElementFinder,
    useFilter: protractor.ElementFinder,
    deleteFilter: protractor.ElementFinder,
    createContract: protractor.ElementFinder,
    systemCatalogCreate: protractor.ElementFinder
};

type FieldsForms = {
  
};

type ColumnHeaders = {
    systemName: protractor.ElementFinder,
    contractName: protractor.ElementFinder,
    catalogName: protractor.ElementFinder,
    catalogUsage: protractor.ElementFinder,
    userApi: protractor.ElementFinder,
    userEmail: protractor.ElementFinder
};

type ColumnObjects = {
    systemName: protractor.ElementArrayFinder,
    contractName: protractor.ElementArrayFinder,
    catalogName: protractor.ElementArrayFinder,
    catalogUsage: protractor.ElementArrayFinder,
    userApi: protractor.ElementArrayFinder,
    UserEmail: protractor.ElementArrayFinder
};

var byDataElementType = new CSSLocator().byDataElementType;
var consts = new Constants();

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {
     
        var buttons: HeaderButtons = {
            resetFilter: element(byDataElementType(consts.kendoButtonResetFilter)),
            saveFilter: element(byDataElementType(consts.kendoButtonSaveFilter)),
            useFilter: element(byDataElementType(consts.kendoButtonUseFilter)),
            deleteFilter: element(byDataElementType(consts.kendoButtonDeleteFilter)),
            createContract: element(byDataElementType(consts.kendoContractButtonCreateContract)),
            systemCatalogCreate: element(byDataElementType(consts.kendoSystemButtonCreate))

        };
        return buttons;
    }

    public columnHeaders(): ColumnHeaders {
        var kendo = new kendoHelper();

        var columns: ColumnHeaders = {
            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader),
            contractName: kendo.getColumnHeaderClickable(consts.kendoContractNameHeader),
            catalogName: kendo.getColumnHeaderClickable(consts.kendoCatalogNameHeader),
            catalogUsage: kendo.getColumnHeaderClickable(consts.kendoCatalogUsageHeader),
            userApi: kendo.getUserColumnHeaderClickable(consts.kendoUserApiHeader),
            userEmail: kendo.getColumnHeaderClickable(consts.kendoUserEmailHeader)

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
            userApi: kendo.getColumnItemLinks(consts.kendoUserApiObject),
            UserEmail: kendo.getColumnItemLinks(consts.kendoUserEmailObject)
        };
        return columns;
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
