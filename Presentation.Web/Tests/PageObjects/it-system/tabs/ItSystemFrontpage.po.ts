import IPageObject = require("../../IPageObject.po");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper");

var ec = protractor.ExpectedConditions;

class ItSystemReference implements IPageObject {

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/edit/1/reference");
    }
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public isDeleteButtonLoaded(): webdriver.until.Condition<boolean> {
        return ec.visibilityOf(this.getDeleteButton());
    }

    public isAlertVisible(): webdriver.until.Condition<boolean> {
        return ec.alertIsPresent();
    }

    public getDeleteButton(): protractor.ElementFinder {
        return element.all(by.css("[class='btn btn-sm ng-binding btn-danger']")).first();
    }

}

export = ItSystemReference;