import IPageObject = require("../../IPageObject.po");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");
var ec = protractor.ExpectedConditions;

class ItSystemReference implements IPageObject {

    private navigationHelper = new NavigationHelper();
    private cssHelper = new CssLocatorHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/system/edit/1/reference");
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

    public getArchiveDutyCommentAsList(): protractor.ElementArrayFinder {
        return element.all(this.cssHelper.byDataElementType("archiveDutyRecommendationComment"));
    }

    public getArchiveDutyCommentInput(): protractor.ElementFinder {
        return this.getArchiveDutyCommentAsList().first();
    }
}

export = ItSystemReference;