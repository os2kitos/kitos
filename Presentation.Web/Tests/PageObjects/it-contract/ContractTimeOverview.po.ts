import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import NavigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import SubNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import CssHelper = require("../../Object-wrappers/CSSLocatorHelper");
import PageObject = require("../IPageObject.po");

class ItContractOverview implements PageObject {
    private ec = protractor.ExpectedConditions;
    private navigationHelper = new NavigationHelper();
    public cssHelper = new CssHelper();

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/contract/plan");
    }

    waitForKendoGrid(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.kendoToolbarWrapper.columnObjects().contractName.first());
    }

    kendoToolbarWrapper = new KendoToolbarWrapper();
    navigationBarWrapper = new NavigationBarWrapper();
    subNavigationBarWrapper = new SubNavigationBarWrapper();
}
export = ItContractOverview;