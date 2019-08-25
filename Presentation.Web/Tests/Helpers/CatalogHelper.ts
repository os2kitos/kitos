import CatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import SystemPage = require("../PageObjects/It-system/Tabs/ItSystemFrontpage.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");
import WaitTimers = require("../Utility/WaitTimers");

var consts = new Constants();
var cssHelper = new CSSLocator();
var pageObject = new CatalogPage();
var systemPage = new SystemPage();
var waitUpTo = new WaitTimers();

class CatalogHelper {


    public static isCreateButtonVisible(expectedEnabledState: boolean) {
        return pageObject.getPage()
            .then(() => {
                return expect(pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.isEnabled()).toBe(expectedEnabledState);
            });
    }

    public static createCatalog(name: string) {
        return pageObject.getPage()
            .then(() => {
                return  pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.click();
            })
            .then(() => {
                return  browser.wait(pageObject.isCreateCatalogAvailable(), waitUpTo.twentySeconds);
            })
            .then(() => {
                return  element(cssHelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
            })
            .then(() => {
                return  element(cssHelper.byDataElementType(consts.saveCatalogButton)).click();
            });
    }

    public static deleteCatalog(name: string) {
        return pageObject.getPage()
            .then(() => {
                return this.waitForKendoGrid();
            })
            .then(() => {
                return this.findCatalogColumnsFor(name).first().click();
            })
            .then(() => {
                return browser.wait(systemPage.isDeleteButtonLoaded(), waitUpTo.twentySeconds);
            })
            .then(() => {
                return systemPage.getDeleteButton().click();
            })
            .then(() => {
                return browser.switchTo().alert().accept();
            });
    }

    public static findCatalogColumnsFor(name: string) {
        return pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects().catalogName, name);
    }

    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);
    }
}

export = CatalogHelper;