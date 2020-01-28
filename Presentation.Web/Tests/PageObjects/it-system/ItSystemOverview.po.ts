import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")
import NavigationHelper = require("../../Utility/NavigationHelper")

class ItSystemOverview implements IPageObject {
    private ec = protractor.ExpectedConditions;
    private navigationHelper = new NavigationHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/system/overview");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public waitForKendoGrid(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.kendoToolbarWrapper.columnObjects().systemName.first());
    }
}

export = ItSystemOverview;