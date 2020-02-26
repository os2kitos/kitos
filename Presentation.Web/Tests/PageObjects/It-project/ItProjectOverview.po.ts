import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")
import NavigationHelper = require("../../Utility/NavigationHelper")

class ItProjectOverview implements IPageObject {
    private navigationHelper = new NavigationHelper();
    private ec = protractor.ExpectedConditions;

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/project/overview");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public waitForKendoGrid(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.kendoToolbarWrapper.columnObjects().projectName.first());
    }

    getCreateProjectButton() {
        return this.kendoToolbarWrapper.headerButtons().createProject;
    }
}

export = ItProjectOverview;