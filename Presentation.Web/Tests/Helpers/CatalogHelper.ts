import CatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");


var consts = new Constants();
var cssHelper = new CSSLocator();
class CatalogHelper {
    public static createCatalog(name: string) {
        var homePage = new CatalogPage();

        homePage.getPage();
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        browser.wait(homePage.isLoginAvailable());
        element(cssHelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(cssHelper.byDataElementType(consts.saveCatalogButton)).click();
    }


    public static deleteCatalog(name: string)
    {
        var homePage = new CatalogPage();
        
        homePage.getPage();
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        browser.wait(homePage.isLoginAvailable());
        element(cssHelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(cssHelper.byDataElementType(consts.saveCatalogButton)).click();

    }
}

export = CatalogHelper;