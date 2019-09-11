import CatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import SystemPage = require("../PageObjects/It-system/Tabs/ItSystemFrontpage.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");
import WaitTimers = require("../Utility/WaitTimers");

class CatalogHelper {
    private static consts = new Constants();
    private static cssHelper = new CSSLocator();
    private static pageObject = new CatalogPage();
    private static systemPage = new SystemPage();
    private static waitUpTo = new WaitTimers();

    public static createCatalog(name: string) {
        return this.pageObject.getPage()
            .then(() => {
                return this.pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.click();
            })
            .then(() => {
                return browser.wait(this.pageObject.isCreateCatalogAvailable(), this.waitUpTo.twentySeconds);
            })
            .then(() => {
                return element(this.cssHelper.byDataElementType(this.consts.nameOfSystemInput)).sendKeys(name);
            })
            .then(() => {
                return element(this.cssHelper.byDataElementType(this.consts.saveCatalogButton)).click();
            });
    }

    public static deleteCatalog(name: string) {
        return this.pageObject.getPage()
            .then(() => {
                return this.waitForKendoGrid();
            })
            .then(() => {
                return this.findCatalogColumnsFor(name).first().click();
            })
            .then(() => {
                return browser.wait(this.systemPage.isDeleteButtonLoaded(), this.waitUpTo.twentySeconds);
            })
            .then(() => {
                return this.systemPage.getDeleteButton().click();
            })
            .then(() => {
                return browser.switchTo().alert().accept();
            });
    }

    public static findCatalogColumnsFor(name: string) {
        return this.pageObject.kendoToolbarWrapper.getFilteredColumnElement(this.pageObject.kendoToolbarWrapper.columnObjects().catalogName, name);
    }

    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(this.pageObject.waitForKendoGrid(), this.waitUpTo.twentySeconds);
    }
}

export = CatalogHelper;