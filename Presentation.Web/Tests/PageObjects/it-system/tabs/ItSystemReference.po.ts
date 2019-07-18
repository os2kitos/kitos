import IPageObject = require("../../IPageObject.po");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper")

var ec = protractor.ExpectedConditions;

class ItSystemEditPo implements IPageObject { 

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/edit/1/reference");
    }
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public isReferenceLoaded(): webdriver.until.Condition<boolean> {
        return ec.visibilityOf(this.kendoToolbarWrapper.headerButtons().editReference);
    }
}

export = ItSystemEditPo;