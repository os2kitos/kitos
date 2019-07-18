import CatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");
import KendoToolBar = require("../object-wrappers/KendoToolbarWrapper")
import WaitTimers = require("../Utility/WaitTimers");

var consts = new Constants();
var cssHelper = new CSSLocator();
var kendo = new KendoToolBar();
var homePage = new CatalogPage();
var waitUpTo = new WaitTimers();

class CatalogHelper {

    public static createCatalog(name: string) {
        homePage.getPage();
        homePage.kendoToolbarWrapper.headerButtons().systemCatalogCreate.click();
        browser.wait(homePage.isLoginAvailable(), waitUpTo.twentySeconds);
        element(cssHelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(cssHelper.byDataElementType(consts.saveCatalogButton)).click();
    }
}

export = CatalogHelper;