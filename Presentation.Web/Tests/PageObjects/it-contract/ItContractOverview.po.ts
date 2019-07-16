import kendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import navigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import subNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import PageObject = require("../IPageObject.po");
import constants = require("../../Utility/Constants");

var consts = new constants();

class ItSystemEditPo implements PageObject {

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/contract/overview");
    }

    kendoToolbarWrapper = new kendoToolbarWrapper();
    navigationBarWrapper = new navigationBarWrapper();
    subNavigationBarWrapper = new subNavigationBarWrapper();

}
export = ItSystemEditPo;