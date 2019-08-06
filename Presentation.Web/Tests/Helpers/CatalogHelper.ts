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

    public static createCatalog(name: string) {
        pageObject.getPage();
        pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.click();
        browser.wait(pageObject.isCreateCatalogAvailable(), waitUpTo.twentySeconds);
        element(cssHelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(cssHelper.byDataElementType(consts.saveCatalogButton)).click();
    }

    public static deleteCatalog(name: string) {
        pageObject.getPage();
        browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);
        pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects().catalogName, name).first().click();
        browser.wait(systemPage.isDeleteButtonLoaded(), waitUpTo.twentySeconds);
        systemPage.getDeleteButton().click().then(() => {
            browser.switchTo().alert().accept();
        });
    }
}

export = CatalogHelper;