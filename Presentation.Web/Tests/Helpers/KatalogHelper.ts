import KatalogPage = require("../PageObjects/it-system/Katalog/ItSystemKatalog.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constant = require("../Utility/Constants");

var consts = new Constant();

class KatalogHelper {

    public static createKatalog(name: string) {
        var homePage = new KatalogPage();
        var CSShelper = new CSSLocator();
        homePage.getPage();
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        element(CSShelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(CSShelper.byDataElementType(consts.saveCatalogButton)).click();
    }


    public static deleteKatalog(name: string)
    {
        var homePage = new KatalogPage();
        var CSShelper = new CSSLocator();
        homePage.getPage();
        homePage.kendoToolbarWrapper.headerButtons().systemKatalogCreate.click();
        element(CSShelper.byDataElementType(consts.nameOfSystemInput)).sendKeys(name);
        element(CSShelper.byDataElementType(consts.saveCatalogButton)).click();

    }
}

export = KatalogHelper;