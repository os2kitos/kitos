import CatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import SystemPage = require("../PageObjects/It-system/Tabs/ItSystemFrontpage.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");
import WaitTimers = require("../Utility/WaitTimers");

class SystemCatalogHelper {
    private static consts = new Constants();
    private static cssHelper = new CSSLocator();
    private static pageObject = new CatalogPage();
    private static systemPage = new SystemPage();
    private static waitUpTo = new WaitTimers();

    public static createSystem(name: string) {
        return SystemCatalogHelper.pageObject.getPage()
            .then(() => {
                return SystemCatalogHelper.pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.click();
            })
            .then(() => {
                return browser.wait(SystemCatalogHelper.pageObject.isCreateCatalogAvailable(), SystemCatalogHelper.waitUpTo.twentySeconds);
            })
            .then(() => {
                return element(SystemCatalogHelper.cssHelper.byDataElementType(SystemCatalogHelper.consts.nameOfSystemInput)).sendKeys(name);
            })
            .then(() => {
                return element(SystemCatalogHelper.cssHelper.byDataElementType(SystemCatalogHelper.consts.saveCatalogButton)).click();
            });
    }

    public static deleteSystem(name: string) {
        return SystemCatalogHelper.pageObject.getPage()
            .then(() => {
                return SystemCatalogHelper.waitForKendoGrid();
            })
            .then(() => {
                return SystemCatalogHelper.findCatalogColumnsFor(name).first().click();
            })
            .then(() => {
                return browser.wait(SystemCatalogHelper.systemPage.isDeleteButtonLoaded(), SystemCatalogHelper.waitUpTo.twentySeconds);
            })
            .then(() => {
                return SystemCatalogHelper.systemPage.getDeleteButton().click();
            })
            .then(() => {
                return browser.switchTo().alert().accept();
            });
    }

    public static findCatalogColumnsFor(name: string) {
        return SystemCatalogHelper.pageObject.kendoToolbarWrapper.getFilteredColumnElement(SystemCatalogHelper.pageObject.kendoToolbarWrapper.columnObjects().catalogName, name);
    }

    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(SystemCatalogHelper.pageObject.waitForKendoGrid(), SystemCatalogHelper.waitUpTo.twentySeconds);
    }
}

export = SystemCatalogHelper;