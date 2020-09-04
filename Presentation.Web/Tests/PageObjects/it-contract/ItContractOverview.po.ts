import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import NavigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import SubNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import CssHelper = require("../../Object-wrappers/CSSLocatorHelper");
import PageObject = require("../IPageObject.po");
import KendoLoaderHelper = require("../../Helpers/KendoLoaderHelper");

class ItContractOverview implements PageObject {
    private navigationHelper = new NavigationHelper();
    private kendoLoaderHelper = new KendoLoaderHelper();
    public cssHelper = new CssHelper();

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/contract/overview");
    }

    waitForKendoGrid() {
        return this.kendoLoaderHelper.waitForKendoGridData(this.kendoToolbarWrapper.columnObjects().contractName.first());
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