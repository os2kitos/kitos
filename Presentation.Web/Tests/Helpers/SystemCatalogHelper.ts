import CatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import SystemPage = require("../PageObjects/It-system/Tabs/ItSystemFrontpage.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");
import WaitTimers = require("../Utility/WaitTimers");
import TestFixture = require("../Utility/TestFixtureWrapper");
import Select2 = require("./Select2Helper");

class SystemCatalogHelper {
    private static consts = new Constants();
    private static cssHelper = new CSSLocator();
    private static pageObject = new CatalogPage();
    private static systemPage = new SystemPage();
    private static waitUpTo = new WaitTimers();
    private static testFixture = new TestFixture();
    

    public static createSystem(name: string) {
        console.log(`Creating system: ${name}`);
        return SystemCatalogHelper.pageObject.getPage()
            .then(() => SystemCatalogHelper.waitForKendoGrid())
            .then(() => SystemCatalogHelper.pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.click())
            .then(() => browser.wait(SystemCatalogHelper.pageObject.isCreateCatalogAvailable(), SystemCatalogHelper.waitUpTo.twentySeconds))
            .then(() => element(SystemCatalogHelper.cssHelper.byDataElementType(SystemCatalogHelper.consts.nameOfSystemInput)).sendKeys(name))
            .then(() => element(SystemCatalogHelper.cssHelper.byDataElementType(SystemCatalogHelper.consts.saveCatalogButton)).click());
    }

    public static deleteSystem(name: string) {
        console.log(`Deleting system: ${name}`);
        return SystemCatalogHelper.pageObject.getPage()
            .then(() => SystemCatalogHelper.waitForKendoGrid())
            .then(() => SystemCatalogHelper.findCatalogColumnsFor(name).first().click())
            .then(() => browser.wait(SystemCatalogHelper.systemPage.isDeleteButtonLoaded(), SystemCatalogHelper.waitUpTo.twentySeconds))
            .then(() => SystemCatalogHelper.systemPage.getDeleteButton().click())
            .then(() => browser.switchTo().alert().accept());
    }


    public static deleteSystemWithoutBrowserWait(name: string) {
        console.log(`Deleting system: ${name}`);
        return SystemCatalogHelper.pageObject.getPage()
            .then(() => SystemCatalogHelper.waitForKendoGrid())
            .then(() => SystemCatalogHelper.findCatalogColumnsFor(name).first().click())
            .then(() => browser.wait(SystemCatalogHelper.systemPage.isDeleteButtonLoaded(),SystemCatalogHelper.waitUpTo.twentySeconds))
            .then(() => SystemCatalogHelper.systemPage.getDeleteButton().click())
            .then(() => this.testFixture.disableAutoBrowserWaits())
            .then(() => browser.switchTo().alert().accept());
    }

    public static setMainSystem(mainSystemName: string, childSystemName: string) {
        console.log(`Deleting system: ${childSystemName}`);
        return SystemCatalogHelper.pageObject.getPage()
            .then(() => SystemCatalogHelper.waitForKendoGrid())
            .then(() => SystemCatalogHelper.findCatalogColumnsFor(childSystemName).first().click())
            .then(() => Select2.searchFor(mainSystemName, "s2id_system-parent"))
            .then(() => Select2.waitForDataAndSelect());
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