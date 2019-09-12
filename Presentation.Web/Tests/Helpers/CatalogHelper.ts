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
        return CatalogHelper.pageObject.getPage()
            .then(() => {
                return CatalogHelper.pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.click();
            })
            .then(() => {
                return browser.wait(CatalogHelper.pageObject.isCreateCatalogAvailable(), CatalogHelper.waitUpTo.twentySeconds);
            })
            .then(() => {
                return element(CatalogHelper.cssHelper.byDataElementType(CatalogHelper.consts.nameOfSystemInput)).sendKeys(name);
            })
            .then(() => {
                return element(CatalogHelper.cssHelper.byDataElementType(CatalogHelper.consts.saveCatalogButton)).click();
            });
    }

    public static deleteCatalog(name: string) {
        return CatalogHelper.pageObject.getPage()
            .then(() => {
                return CatalogHelper.waitForKendoGrid();
            })
            .then(() => {
                return CatalogHelper.findCatalogColumnsFor(name).first().click();
            })
            .then(() => {
                return browser.wait(CatalogHelper.systemPage.isDeleteButtonLoaded(), CatalogHelper.waitUpTo.twentySeconds);
            })
            .then(() => {
                return CatalogHelper.systemPage.getDeleteButton().click();
            })
            .then(() => {
                return browser.switchTo().alert().accept();
            });
    }

    public static checkMigrationButtonVisibility(isVisible : boolean) {
        return this.pageObject.getPage().then(() => {
            return this.pageObject.waitForKendoGrid();
        }).then(() => {
            return this.pageObject.kendoToolbarWrapper.columnObjects().usedByName.first().click();
        }).then(() => {
            return expect(element(this.cssHelper.byDataElementType(this.consts.moveSystemButton)).isPresent()).toBe(isVisible);
        });
    }


    public static findCatalogColumnsFor(name: string) {
        return CatalogHelper.pageObject.kendoToolbarWrapper.getFilteredColumnElement(CatalogHelper.pageObject.kendoToolbarWrapper.columnObjects().catalogName, name);
    }

    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(CatalogHelper.pageObject.waitForKendoGrid(), CatalogHelper.waitUpTo.twentySeconds);
    }
}

export = CatalogHelper;