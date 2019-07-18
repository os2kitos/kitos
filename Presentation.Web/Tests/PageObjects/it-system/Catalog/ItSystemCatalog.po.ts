import IPageObject = require("../../IPageObject.po");
import KendoToolbarHelper = require("../../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper")
import CssLocatorHelper = require("../../../object-wrappers/CSSLocatorHelper");
import Constants = require("../../../Utility/Constants");
var consts = new Constants();
var ec = protractor.ExpectedConditions;
var byDataElementType = new CssLocatorHelper().byDataElementType;


class ItSystemCatalog implements IPageObject {

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/catalog");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();
    public createCatalogForm = element(byDataElementType(consts.catalogCreateForm));


    public isLoginAvailable(): webdriver.until.Condition<boolean> {
        return ec.visibilityOf(this.createCatalogForm);
    }

    public 

}

export = ItSystemCatalog;