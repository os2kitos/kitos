import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")
import NavigationHelper = require("../../Utility/NavigationHelper")

class ReportOverview implements IPageObject {
    private navigationHelper = new NavigationHelper();
    private ec = protractor.ExpectedConditions;

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("#/rapporter/overblik");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public waitForKendoGrid(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.kendoToolbarWrapper.columnObjects().reportName.first());
    }

    getCreateReportButton() {
        return this.kendoToolbarWrapper.headerButtons().createReport;
    }
}

export = ReportOverview;