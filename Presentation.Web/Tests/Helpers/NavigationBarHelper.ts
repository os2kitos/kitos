import NavigationBarWrapper = require("../object-wrappers/NavigationBarWrapper");

class NavigationBarHelper {
    private navigationBar = new NavigationBarWrapper();
    public dropDownExpand() {
        return this.navigationBar.dropDownMenu.dropDownElement.click().then(() => browser.waitForAngular());
    }

    public logout() {
        return this.navigationBar.dropDownMenu.logOut.click().then(() => browser.waitForAngular());
    }

    public isMyProfileDisplayed(): webdriver.promise.Promise<Boolean>{
        return this.navigationBar.dropDownMenu.myProfile.isPresent();
    }

    public isGlobalAdminDisplayed(): webdriver.promise.Promise<Boolean> {
        return this.navigationBar.dropDownMenu.globalAdmin.isPresent();
    }

    public isLocalAdminDisplayed(): webdriver.promise.Promise<Boolean> {
        return this.navigationBar.dropDownMenu.localAdmin.isPresent();
    }

    public changeOrg() {
        return this.navigationBar.dropDownMenu.changeOrg.click().then(() => browser.waitForAngular());
    }
}
export = NavigationBarHelper;