import IPageObject = require("../IPageObject.po");
import kendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import navigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import subNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");

class ItSystemEditPo implements IPageObject { 

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/overview");
    }

    kendoToolbarWrapper = new kendoToolbarWrapper();
    navigationBarWrapper = new navigationBarWrapper();
    subNavigationBarWrapper = new subNavigationBarWrapper();

}

export = ItSystemEditPo;