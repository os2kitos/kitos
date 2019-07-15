import KatalogPage = require("../PageObjects/it-system/Katalog/ItSystemKatalog.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");

class KatalogHelper {


    public static createKatalog(name: string) {
        var homePage = new KatalogPage();
        var CSShelper = new CSSLocator();
        homePage.getPage();
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        element(CSShelper.byDataElementType("nameOfItSystemInput")).sendKeys(name);
        element(CSShelper.byDataElementType("itKatalogSaveButton")).click();
    }


    public static deleteKatalog(name: string)
    {
        var homePage = new KatalogPage();
        var CSShelper = new CSSLocator();
        homePage.getPage();
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        element(CSShelper.byDataElementType("nameOfItSystemInput")).sendKeys(name);
        element(CSShelper.byDataElementType("itKatalogSaveButton")).click();

    }
}

export = KatalogHelper;