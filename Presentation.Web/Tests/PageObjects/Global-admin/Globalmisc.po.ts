import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import NavigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import SubNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import CssLocatorHelper = require("../../Object-wrappers/CSSLocatorHelper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import PageObject = require("../IPageObject.po");
import Constants = require("../../Utility/Constants");

class GlobalMisc implements PageObject {
    private navigationHelper = new NavigationHelper();
    private ec = protractor.ExpectedConditions;
    private consts = new Constants();
    private byDataElementType = new CssLocatorHelper().byDataElementType;

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/global-admin/misc");
    }

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    public waitForKleChangesButtonToBeActive() {
        return this.ec.elementToBeClickable(element(this.byDataElementType(this.consts.kleChangesButton)));
    }

    public waitForKleUpdateButtonToBeActive() {
        return this.ec.elementToBeClickable(element(this.byDataElementType(this.consts.kleChangesButton)));
    }

    public waitForKleDownloadLink() {
        return this.ec.presenceOf(element(this.byDataElementType(this.consts.KleDownloadAnchor)));
        //   return this.ec.elementToBeClickable(element(this.byDataElementType(this.consts.KleDownloadAnchor)));
    }
}
export = GlobalMisc;