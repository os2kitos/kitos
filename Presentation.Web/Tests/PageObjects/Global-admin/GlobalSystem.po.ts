import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import NavigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import SubNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import CssLocatorHelper = require("../../Object-wrappers/CSSLocatorHelper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import PageObject = require("../IPageObject.po");
import Constants = require("../../Utility/Constants");
import WaitTimers = require("../../Utility/WaitTimers");
var waitUpTo = new WaitTimers();

class GlobalSystem implements PageObject {
    private navigationHelper = new NavigationHelper();
    private ec = protractor.ExpectedConditions;
    private consts = new Constants();
    private byDataElementType = new CssLocatorHelper().byDataElementType;

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/global-admin/system");
    }
}
export = GlobalSystem;