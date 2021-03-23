import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")
import NavigationHelper = require("../../Utility/NavigationHelper")
import KendoLoaderHelper = require("../../Helpers/KendoLoaderHelper")

class ItProjectOverview implements IPageObject {
    private navigationHelper = new NavigationHelper();
    private kendoLoaderHelper = new KendoLoaderHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/project/overview");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

     waitForKendoGrid() {
        return this.kendoLoaderHelper.waitForKendoGridData(this.kendoToolbarWrapper.columnObjects().projectName.first());
     }

    getCreateProjectButton() {
        return this.kendoToolbarWrapper.headerButtons().createProject;
    }
}

export = ItProjectOverview;