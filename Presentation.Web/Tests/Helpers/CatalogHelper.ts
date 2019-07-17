import CatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");


var consts = new Constants();

class CatalogHelper {
    public static createCatalog(name: string) {
        var homePage = new CatalogPage();
        var CSShelper = new CSSLocator();
        homePage.getPage();
        browser.wait(homePage.isLoginAvailable());
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        element(CSShelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(CSShelper.byDataElementType(consts.saveCatalogButton)).click();
    }


    public static deleteCatalog(name: string)
    {
        var homePage = new CatalogPage();
        var CSShelper = new CSSLocator();
        homePage.getPage();
        browser.wait(homePage.isLoginAvailable());
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        element(CSShelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(CSShelper.byDataElementType(consts.saveCatalogButton)).click();

    }
}

export = CatalogHelper;