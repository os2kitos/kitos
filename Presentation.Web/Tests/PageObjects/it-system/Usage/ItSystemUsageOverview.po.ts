import IPageObject = require("../../IPageObject.po");
import KendoToolbarHelper = require("../../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper")
import NavigationHelper = require("../../../Utility/NavigationHelper")
import KendoLoaderHelper = require("../../../Helpers/KendoLoaderHelper")

class ItSystemOverview implements IPageObject {
    private navigationHelper = new NavigationHelper();
    private ec = protractor.ExpectedConditions;
    private kendoLoaderHelper = new KendoLoaderHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#!/system/overview");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public waitForKendoGrid() {
        return this.kendoLoaderHelper.waitForKendoGridData(this.kendoToolbarWrapper.columnObjects().systemName.first());
    }
}

export = ItSystemOverview;