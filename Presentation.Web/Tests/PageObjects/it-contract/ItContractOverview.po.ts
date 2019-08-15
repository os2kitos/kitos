import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import NavigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import SubNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import PageObject = require("../IPageObject.po");

class ItContractOverview implements PageObject {

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/contract/overview");
    }

    kendoToolbarWrapper = new KendoToolbarWrapper();
    navigationBarWrapper = new NavigationBarWrapper();
    subNavigationBarWrapper = new SubNavigationBarWrapper();
}
export = ItContractOverview;