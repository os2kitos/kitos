import IPageObject = require("../../IPageObject.po");
import Constants = require("../../../Utility/Constants");
import CssLoactor = require("../../../Object-wrappers/CSSLocatorHelper");
import KendoToolbarHelper = require("../../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper")

var consts = new Constants();
var ec = protractor.ExpectedConditions;
var byDataElementType = new CssLoactor().byDataElementType;

class ItSystemEditPo implements IPageObject { 

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/edit/1/reference");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();
    public refrenceEditButton = element(byDataElementType(consts.refrenceEditButton));


    public isReferenceLoaded(): webdriver.until.Condition<boolean> {
        return ec.visibilityOf(this.refrenceEditButton);
    }


}

export = ItSystemEditPo;