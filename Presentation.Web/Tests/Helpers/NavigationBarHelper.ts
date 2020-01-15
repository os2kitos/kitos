import NavigationBarWrapper = require("../object-wrappers/NavigationBarWrapper");

class NavigationBarHelper {
    private navigationBar = new NavigationBarWrapper();
    public dropDownExpand() {
        return  this.navigationBar.dropDownMenu.dropDownElement.click();
    }

    public logout() {
        return this.navigationBar.dropDownMenu.logOut.click();
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

}

export = NavigationBarHelper;