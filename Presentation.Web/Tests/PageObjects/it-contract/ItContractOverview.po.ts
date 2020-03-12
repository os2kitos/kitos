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
        return this.navigationHelper.getPage("/#/contract/overview");
    }

    waitForKendoGrid(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.kendoToolbarWrapper.columnObjects().contractName.first());
    }

    getCreateContractButton() {
        return this.kendoToolbarWrapper.headerButtons().createContract;
    }

    getSaveContractButton() {
        return element(this.cssHelper.byDataElementType("contractSaveButton"));
    }

    getContractNameInputField() {
        return element(this.cssHelper.byDataElementType("contractNameInput"));
    }

    kendoToolbarWrapper = new KendoToolbarWrapper();
    navigationBarWrapper = new NavigationBarWrapper();
    subNavigationBarWrapper = new SubNavigationBarWrapper();
}
export = ItContractOverview;