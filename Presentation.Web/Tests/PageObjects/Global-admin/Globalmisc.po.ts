import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import NavigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import SubNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import PageObject = require("../IPageObject.po");

class GlobalMisc implements PageObject {
    private navigationHelper = new NavigationHelper();
    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/global-admin/misc");
    }
}
export = GlobalMisc;